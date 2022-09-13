using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Populators;
using Game.Providers;
using Game.Controllers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    public class GameController : MonoBehaviour
    {
        public static readonly Regex PATTERN = new Regex("([0-9]+\\.)");
        public static GameController instance => _instance;
        private static GameController _instance;

        public PlayerController whitePlayer;
        public PlayerController blackPlayer;

        #region Stockfish
        public bool stockFishSearching = false;
        public Thread stockfishThread = null;
        public System.Diagnostics.Process stockfishProcess = null;
        public ConcurrentQueue<StockfishSearch> stockfishQueue = new ConcurrentQueue<StockfishSearch>();
        #endregion

        #region References
        public PiecesPrefabs prefabsWhite;
        public PiecesPrefabs prefabsBlack;
        public TileRenderer tileRenderer;
        #endregion

        #region State
        public BoardState state;
        public Vector2Int selectedPiece;
        public List<GameObject> piecesToDestroy = new List<GameObject>();
        public List<Move> selectedPieceMoves;
        public bool waiting = false;
        public BoardStateBuffer buffer;
        public char promotionTarget = '?';
        #endregion

        #region Populators
        public PieceMovePopulator populatorPawn = null;
        public PieceMovePopulator populatorKing = null;
        public PieceMovePopulator populatorKingCastler = null;
        public PieceMovePopulator populatorQueen = null;
        public PieceMovePopulator populatorBishop = null;
        public PieceMovePopulator populatorRook = null;
        public PieceMovePopulator populatorRookCastler = null;
        public PieceMovePopulator populatorKnight = null;
        #endregion

        #region Providers
        public AttackLinesProvider providerStraight = null;
        public AttackLinesProvider providerDiagonal = null;
        public AttackLinesProviderPawn providerPawn = null;
        public AttackLinesProviderKing providerKing = null;
        public AttackLinesProviderKnight providerKnight = null;
        #endregion

        #region UI
        [Header("UI")]
        public TMP_Text labelGameState;
        public TMP_Text labelCurrentPlayer;
        public TMP_Text labelLastMove;
        public TMP_Text labelLastMoves;
        public GameObject menu;
        public GameObject hud;
        public TMP_InputField input;
        public Slider animationLengthSlider;
        public TMP_Text animationLengthDisplay;
        public Toggle toggleAutoPlay;
        public Toggle toggleSoundEffects;
        public TMP_InputField inputMove;
        public GameObject promotionMenu;
        public TMP_Dropdown pcModeDropdown;
        public TMP_InputField stockfishPath;
        #endregion

        #region Settings
        [Header("SETTINGS")]
        public bool moveAnimation = true;
        public float moveAnimationLength = 1f;
        public float captureDestroyAfter = 2f;
        public bool autoPlay = true;
        public bool soundEffects = true;
        #endregion

        #region Sounds
        [Header("SOUNDS")]
        public AudioSource[] sources;
        public int nextSource = 0;

        public AudioClip soundOnClick;
        public AudioClip soundOnMove;
        public AudioClip soundOnCapture;
        public AudioClip soundOnCheck;
        public AudioClip soundOnMate;
        #endregion

        #region Auto Player
        public List<string> autoPlayerNextMoves = new List<string>();
        #endregion

        public void OnEnable()
        {
            if(instance != null && !instance.Equals(null))
            {
                if(instance == this)
                    return;

                Debug.LogError("Attempted to create two coexisting GameController, which is not allowed!");

                if(instance.gameObject == gameObject)
                    Destroy(instance);
                else
                    Destroy(instance.gameObject);
            }

            _instance = this;

            populatorPawn = new PieceMovePopulatorPawn();
            populatorKnight = new PieceMovePopulatorKnight();
            populatorRook = new PieceMovePopulatorRook();
            populatorBishop = new PieceMovePopulatorBishop();
            populatorQueen = new PieceMovePopulatorQueen();
            populatorKing = new PieceMovePopulatorKing();
            populatorKingCastler = new PieceMovePopulatorKingCastler();
            populatorRookCastler = new PieceMovePopulatorRookCastler();

            providerStraight = new AttackLinesProviderStraight();
            providerDiagonal = new AttackLinesProviderDiagonal();
            providerPawn = new AttackLinesProviderPawn();
            providerKing = new AttackLinesProviderKing();
            providerKnight = new AttackLinesProviderKnight();

            DefaultMode();

            //Keep running after reload
            if(state != null)
            {
                state.Reload();
                UpdateGameState();
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos() 
        {
            foreach(Piece piece in state.pieceList)
            {
                Handles.color = Color.red;
                Handles.Label(piece.gameObject.transform.position + Vector3.up, $"<{piece.populator.GetType().Name}>");
            }
        }
        #endif

        public void OnDisable()
        {
            StopStockFish();
        }
        
        private void Start() 
        {
            state = new BoardState();

            stockfishPath.text = Application.dataPath + "/Resources/stockfish"; 

            Restart();    
            DefaultMode();
            OpenMenu();
        }

        private void Update() 
        {
            bool white = state.currentPlayer == 'w';
            if(!menu.activeInHierarchy)
                (white ? whitePlayer : blackPlayer).PlayerTurn(white);
            
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(menu.activeInHierarchy)
                    CloseMenu();
                else
                    OpenMenu();
            }

            if(Input.GetKey(KeyCode.LeftShift))
            {
                if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    Restart("r3k2r/8/8/8/8/8/8/R3K2R w kqKQ - 0 0");
                }
            }

            moveAnimationLength = animationLengthSlider.value;
            moveAnimation = moveAnimationLength > 0;
            animationLengthDisplay.text = moveAnimationLength.ToString("0.0");
        }

        #region Stockfish Test
        public void StartStockFish()
        {
            stockfishThread = new Thread(StockFishThread);
            stockfishThread.Start();
        }

        public void StopStockFish()
        {
            if(stockfishProcess == null)
                return;

            stockfishProcess.Close();
            stockfishProcess = null;
            stockfishThread = null;
        }

        public IEnumerator StockFishSearch(Action<string> callback)
        {
            if(stockfishProcess == null)
                StartStockFish();


            stockFishSearching = true;
            StockfishSearch search = new StockfishSearch();
            search.fen = state.GetFEN();
            
            string move = null;
            search.callback = (m) => move = m;
            stockfishQueue.Enqueue(search);

            while(move == null)
                yield return new WaitForSeconds(0.05f);

            callback.Invoke(move);
            stockFishSearching = false;
        }

        public void StockFishThread()
        {
            string fen = state.GetFEN();

            stockfishProcess = new System.Diagnostics.Process();
            stockfishProcess.StartInfo.FileName = stockfishPath.text;
           
            Debug.Log($"[Stockfish] Starting stock fish at {stockfishPath.text}"); 

            stockfishProcess.StartInfo.UseShellExecute = false;
            stockfishProcess.StartInfo.RedirectStandardInput = true;
            stockfishProcess.StartInfo.RedirectStandardOutput = true;
            stockfishProcess.StartInfo.CreateNoWindow = true;
            stockfishProcess.Start();

            Debug.Log("[Stockfish] Running...");  

            while(stockfishProcess != null)
            {
                if(stockfishQueue.TryDequeue(out var search))
                {
                    stockfishProcess.StandardInput.WriteLine($"position fen {search.fen}");
                    stockfishProcess.StandardInput.WriteLine($"go movetime {search.time}");
                
                    while(stockfishProcess != null)
                    {
                        string s = stockfishProcess.StandardOutput.ReadLine();

                        Debug.Log($"[Stockfish] >> {s}");

                        if(s.StartsWith("bestmove"))
                        {
                            search.callback?.Invoke(s.Split(' ')[1]);
                            break;
                        }
                    }
                }
            }

            Debug.Log("[Stockfish] Stopped!");  

            stockfishProcess?.Close();
            stockfishProcess = null;
            stockfishThread = null;
        }
        #endregion

        public void Restart(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 0")
        {
            waiting = false;
            StopAllCoroutines();
            LoadFEN(fen);
            promotionMenu.SetActive(false);
            stockfishQueue.Clear();

            foreach(GameObject o in piecesToDestroy)
                Destroy(o);

            piecesToDestroy.Clear();
        }

        #region UI
        public void OpenMenu()
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;

            hud.SetActive(false);
            menu.SetActive(true);

            tileRenderer.ClearPossibleMoves();
            tileRenderer.ClearSelectedTile();
            selectedPiece = new Vector2Int(-1, -1);

            toggleAutoPlay.isOn = autoPlay;
            toggleSoundEffects.isOn = soundEffects;
        }

        public void CloseMenu()
        {
            autoPlay = toggleAutoPlay.isOn;
            soundEffects = toggleSoundEffects.isOn;
            selectedPiece = new Vector2Int(-1, -1);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 1/50f;

            menu.SetActive(false);
            hud.SetActive(true);
        }

        public void CopyLastMoves()
        {
            TextEditor editor = new TextEditor
            {
                text = labelLastMoves.text.Trim()
            };
            editor.SelectAll();
            editor.Copy();
        } 
        
        public void PlayPGN()
        {
            Restart();
            AutoPlayMode(input.text);
            CloseMenu();
        }

        public void OnInputMove()
        {
            if(inputMove.text.Length > 0)
            {
                DoMove(inputMove.text);
                inputMove.text = "";
            }
        }

        public void SetPromotionTarget(int target)
        {
            promotionTarget = new char[]{'Q', 'B', 'N', 'R'}[target];
            promotionMenu.SetActive(false);
        }
        #endregion

        #region Game Modes
        /// <summary>
        /// Play full PGN
        /// </summary>
        /// <param name="pgn"></param>
        /// <returns></returns>
        public void AutoPlayMode(string pgn)
        {
            string[] moves = pgn.Split(" ");
            autoPlayerNextMoves.Clear();

            for(int i = 0; i < moves.Length; i ++)
            {
                if(!PATTERN.IsMatch(moves[i]))  
                {
                    autoPlayerNextMoves.Add(moves[i]);
                }
            }

            whitePlayer = blackPlayer = new AutoPlayerController();      
        }

        public void DefaultMode()
        {
            whitePlayer = blackPlayer = new DefaultPlayerController();
        }

        public void PlayerVsPCMode()
        {
            whitePlayer = new DefaultPlayerController();
            blackPlayer = GetPCStrategy();
        }

        public void PCVsPCMode()
        {
            whitePlayer = blackPlayer = GetPCStrategy();
        }

        public PlayerController GetPCStrategy()
        {
            switch(pcModeDropdown.value)
            {
                case 0:
                    return new PCRandomPlayerController();
                case 1:
                    return new PCSimplePlayerController();
                default:
                    return new PCStockfishPlayerController();
            }
        }
        #endregion

        #region Actions
        public void LoadFEN(string fen)
        {
            state.LoadFEN(fen);
            ClearInterface();
        }
        /// <summary>
        /// Update current game state
        /// </summary>
        public void UpdateGameState()
        {
            buffer = state.GenerateStateBuffer(state.currentPlayer == 'w');

            labelCurrentPlayer.SetText(state.currentPlayer == 'w' ? "- White -" : "- Black -");
            
            labelGameState.SetText("Playing");
            labelGameState.color = Color.green;

            if(buffer.inCheck)
            {
                if(buffer.inCheckMate)
                {
                    labelGameState.SetText("Mate");
                    labelGameState.color = Color.red;
                }
                else
                {
                    labelGameState.SetText("Check");
                    labelGameState.color = Color.yellow;
                }
            }
            else if(buffer.inStaleMate)
            {
                labelGameState.SetText("Stale-Mate");
                labelGameState.color = Color.red;
            }
        }

        /// <summary>
        /// Clear UI
        /// </summary>
        public void ClearInterface()
        {
            selectedPiece = new Vector2Int(-1, -1);
            GameController.instance.UpdateGameState();
            GameController.instance.labelLastMove.text = "-/-";
            GameController.instance.labelLastMoves.text = "";
            tileRenderer.ClearPossibleMoves();
            tileRenderer.ClearSelectedTile();
        }

        /// <summary>
        /// Parses a SAN, then perfoms a move
        /// </summary>
        /// <param name="san"></param>
        public bool DoMove(string san, Action callback = null, bool uci = false)
        {
            Debug.Log("[Auto-Playing]: " + san);

            Move move = uci ? state.GetUCIMove(san) : state.GetSANMove(san);

            List<Move> matching = GetMatchingMovesFromBuffer(move, buffer);
            
            if(matching.Count == 1)
            {
                //Auto promotion if needed
                Move toDo = matching[0];
                toDo.promoteTo = move.promoteTo;

                DoMove(toDo, callback);
                return true;
            }
            else if(matching.Count == 0)
                Debug.LogError($"Move {san} is invalid!");
            else if(matching.Count > 1)
                Debug.LogError($"Move {san} is ambiguous!");

            return false;
        }

        /// <summary>
        /// Performs a Move on the board (with animation)
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public void DoMove(Move move, Action callback = null)
        {
            GameController.instance.waiting = true;
            StartCoroutine(DoMoveInternal(move, callback));
        }

        /// <summary>
        /// Internal move coroutine
        /// </summary>
        /// <param name="move"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator DoMoveInternal(Move move, Action callback = null)
        {   
            #region Preparation
            waiting = true;
            Piece piece = state[move.from.x, move.from.z];

            Piece pieceCaptured = state[move.to.x, move.to.z];

            if(pieceCaptured != null)
            {
                if(pieceCaptured.gameObject != null)
                    pieceCaptured.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                else
                    pieceCaptured = null;
            }

            Vector3 from = piece.gameObject.transform.position;
            Vector3 to = new Vector3(move.to.x, 0, move.to.z);

            Piece castlingRook = null;
            Vector3 castlingFrom = default;
            Vector3 castlingTo = default;

            if(move.isCastling)
            {
                castlingRook = state[move.isCastlingQueenside ? 0 : 7, move.to.z];
                castlingFrom = castlingRook.gameObject.transform.position;
                castlingTo = new Vector3(move.isCastlingQueenside ? 3 : 5, 0, move.to.z);
            }
            #endregion

            #region Animation
            float f = 0;
            bool jumped = false;

            if(moveAnimation)
            {
                while(f < 1)
                {   
                    piece.gameObject.transform.position = Vector3.Lerp(from, to, f);

                    if(pieceCaptured != null)
                    {
                        if(!jumped && Vector3.Distance(pieceCaptured.gameObject.transform.position, piece.gameObject.transform.position) < 0.6f)
                        {
                            jumped = true;

                            Rigidbody rb = pieceCaptured.gameObject.GetComponent<Rigidbody>();
                            
                            Vector3 dir = (pieceCaptured.gameObject.transform.position - piece.gameObject.transform.position).normalized;
                            rb.velocity += new Vector3(dir.x, 0.2f, dir.y) * 5f;
                            rb.angularVelocity += new Vector3(
                                UnityEngine.Random.Range(-180f, 180f),
                                UnityEngine.Random.Range(-180f, 180f),
                                UnityEngine.Random.Range(-180f, 180f)
                            );
                        }
                    }

                    if(castlingRook != null)
                        castlingRook.gameObject.transform.position = Vector3.Lerp(castlingFrom, castlingTo, f);

                    f += Time.deltaTime / moveAnimationLength;
                    yield return new WaitForEndOfFrame();
                }
            }
            #endregion
            
            #region Complete Move
            state[move.from.x, move.from.z] = null;
            state[move.to.x, move.to.z] = piece;
            piece.position = new Vector2Int(move.to.x, move.to.z);
            piece.gameObject.transform.position = to;
            #endregion

            if(pieceCaptured != null)
            {
                piecesToDestroy.Add(pieceCaptured.gameObject);
            }

            #region EnPassant Move / Capature
            if(piece.type == 'p' || piece.type == 'P')
            {
                if(move.to.x == state.enPassantPosition.x && move.to.z == state.enPassantPosition.y)
                {
                    move.isCapture = true;

                    //Destroy enpassant
                    int y = state.enPassantPosition.y == 2 ? 3 : 4;

                    pieceCaptured = state[move.to.x, y];
                    state[move.to.x, y] = null;
                    pieceCaptured.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                }

                state.enPassantPosition = new Vector2Int(-1, -1);
                
                if(Math.Abs(move.from.z - move.to.z) == 2)
                {
                    state.enPassantPosition = new Vector2Int(move.from.x, (move.from.z + move.to.z)/2);
                }
            }
            else
                state.enPassantPosition = new Vector2Int(-1, -1);
            #endregion
            
            #region Castling
            if(castlingRook != null) //move the rook
            {
                castlingRook.position = new Vector2Int(move.isCastlingQueenside ? 3 : 5, move.to.z);
                castlingRook.gameObject.transform.position = castlingTo;

                state[move.isCastlingQueenside ? 0 : 7, move.from.z] = null;
                state[move.isCastlingQueenside ? 3 : 5, move.from.z] = castlingRook;
            }

            //Can't castle anymore
            if(piece.populator is PieceMovePopulatorKingCastler)
            {
                if(piece.isWhite)
                {
                    state.whiteCastleKingside = false;
                    state.whiteCastleQueenside = false;
                }
                else
                {
                    state.blackCastleKingside = false;
                    state.blackCastleQueenside = false;
                }

                piece.populator = GetPopulator(piece);
            }

            if(piece.populator is PieceMovePopulatorRookCastler)
            {
                if(piece.isWhite)
                {
                    if(from.x == 0)
                        state.whiteCastleQueenside = false;
                    else
                        state.whiteCastleKingside = false;
                }
                else
                {
                    if(from.x == 0)
                        state.blackCastleQueenside = false;
                    else
                        state.blackCastleKingside = false;
                }

                piece.populator = GetPopulator(piece);
            }
            #endregion

            #region Promotion
            if(move.isPromotion)
            {
                if(move.promoteTo == '?' || move.promoteTo == 0)
                {
                    promotionTarget = '?';

                    promotionMenu.SetActive(true);

                    while(promotionTarget == '?')
                        yield return new WaitForSeconds(0.1f);

                    move.promoteTo = promotionTarget;
                }

                piece.SetType(piece.isWhite ? Char.ToUpper(move.promoteTo) : Char.ToLower(move.promoteTo));   
            }
            #endregion

            #region End / Update Game State
            state.currentPlayer = state.currentPlayer == 'w' ? 'b' : 'w';
            BoardStateBuffer pre = buffer;
            UpdateGameState(); 

            state.halfMoveCountSinceLastCapture ++;
            if(move.isCapture)
                state.halfMoveCountSinceLastCapture = 0;

            if(state.currentPlayer == 'b')
                state.fullMoveCount ++;

            move.isCheckMate = buffer.inCheckMate;
            move.isCheck = buffer.inCheck;

            if(move.isCheckMate)
                PlaySound(soundOnMate);
            else if(move.isCheck)
                PlaySound(soundOnCheck);
            else if(move.isCapture)
                PlaySound(soundOnCapture);
            else
                PlaySound(soundOnMove);

            string s = SimplifyMove(move, pre).ToString();
            labelLastMove.SetText(s);
            labelLastMoves.text += $" {s}";
            state.moves.Add(s);

            waiting = false;

            //Call callback if callback is not null
            callback?.Invoke();
            #endregion
            
            #region Destroy Captured piece
            if(pieceCaptured != null)
            {
                if(moveAnimation)
                {
                    float t = 0;
                    while(t < 1)
                    {
                        float inv = 1 - t;
                        pieceCaptured.gameObject.transform.localScale = new Vector3(inv, inv, inv);
                        t += Time.deltaTime/captureDestroyAfter;
                        yield return new WaitForEndOfFrame();
                    }
                    
                    //yield return new WaitForSeconds(captureDestroyAfter);
                }

                piecesToDestroy.Remove(pieceCaptured.gameObject);
                Destroy(pieceCaptured.gameObject);
            }      
            #endregion
        }
        #endregion

        #region Utils
        /// <summary>
        /// Get all possible moves that match with a filter
        /// </summary>
        /// <param name="move"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public List<Move> GetMatchingMovesFromBuffer(Move move, BoardStateBuffer buffer)
        {
            List<Move> matching = new List<Move>();

            foreach(var k in buffer.moves.Values)
            {
                foreach(var m in k)
                {
                    if(m.Matches(move))
                        matching.Add(m);
                }
            }

            return matching;
        }

        /// <summary>
        /// Simplify move for SAN notation
        /// </summary>
        /// <param name="move"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public Move SimplifyMove(Move move, BoardStateBuffer buffer)
        {
            move.isCheck = this.buffer.inCheck;
            move.isCheckMate = this.buffer.inCheckMate;

            Move simplified = move;
            
            simplified.from = Pos.EMPTY;

            if(!move.isCapture || move.piece != 'P')
            {
                if(GetMatchingMovesFromBuffer(simplified, buffer).Count == 1)
                    return simplified;
            }

            simplified.from.file = move.from.file;

            if(GetMatchingMovesFromBuffer(simplified, buffer).Count == 1)
                return simplified;

            simplified.from = Pos.EMPTY;
            simplified.from.rank = move.from.rank;

            if(GetMatchingMovesFromBuffer(simplified, buffer).Count == 1)
                return simplified;

            simplified.from = move.from;
            return move;
        }

        /// <summary>
        /// Get the corresponding prefab for a piece type
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public GameObject GetPrefab(char piece)
        {
            switch (piece)
            {
                case 'P': return prefabsWhite.pawn;
                case 'N': return prefabsWhite.knight;
                case 'B': return prefabsWhite.bishop;
                case 'R': return prefabsWhite.rook;
                case 'Q': return prefabsWhite.queen;
                case 'K': return prefabsWhite.king;

                case 'p': return prefabsBlack.pawn;
                case 'n': return prefabsBlack.knight;
                case 'b': return prefabsBlack.bishop;
                case 'r': return prefabsBlack.rook;
                case 'q': return prefabsBlack.queen;
                case 'k': return prefabsBlack.king;

                default: return null;
            }   
        }

        /// <summary>
        /// Get the piece move populator for a piece type
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public PieceMovePopulator GetPopulator(Piece piece)
        {
            switch (piece.type)
            {
                case 'p': case 'P': return populatorPawn;
                case 'r':
                    if(piece.position.x == 0)
                        return state.blackCastleQueenside ? populatorRookCastler : populatorRook;
                    else if(piece.position.x == 7)
                        return state.blackCastleKingside ? populatorRookCastler : populatorRook;
                    else
                        return populatorRook;

                case 'R':
                    if(piece.position.x == 0)
                        return state.whiteCastleQueenside ? populatorRookCastler : populatorRook;
                    else if(piece.position.x == 7)
                        return state.whiteCastleKingside ? populatorRookCastler : populatorRook;
                    else
                        return populatorRook;

                case 'q': case 'Q': return populatorQueen;
                case 'k': 
                    if(state.blackCastleKingside || state.blackCastleQueenside)
                        return populatorKingCastler;
                    else
                        return populatorKing;
                case 'K':
                    if(state.whiteCastleKingside || state.whiteCastleQueenside)
                        return populatorKingCastler;
                    else
                        return populatorKing;

                case 'n': case 'N': return populatorKnight;
                case 'b': case 'B': return populatorBishop;

                default: return null;
            }   
        }

        /// <summary>
        /// Load all attack line providers for a piece
        /// </summary>
        /// <param name="piece"></param>
        public void LoadProviders(Piece piece)
        {
            switch (piece.type)
            {
                case 'r': case 'R': 
                    piece.attackLineProviders += providerStraight.GetAttackLines;
                    break;

                case 'b': case 'B': 
                    piece.attackLineProviders += providerDiagonal.GetAttackLines;
                    break;
                
                case 'q': case 'Q': 
                    piece.attackLineProviders += providerStraight.GetAttackLines;
                    piece.attackLineProviders += providerDiagonal.GetAttackLines;
                    break;

                case 'p': case 'P':
                    piece.attackLineProviders += providerPawn.GetAttackLines;
                    break;

                case 'k': case 'K':
                    piece.attackLineProviders += providerKing.GetAttackLines;
                    break;

                case 'n': case 'N':
                    piece.attackLineProviders += providerKnight.GetAttackLines;
                    break;
            }
        }
        #endregion 
    
        #region Sound Effects
        public void PlaySound(AudioClip clip)
        {
            if(clip == null || !soundEffects)
                return;

            nextSource = (nextSource + 1)%sources.Length;

            AudioSource source = sources[nextSource];
            source.clip = clip;
            source.Play();
        }
        #endregion
    }   
}