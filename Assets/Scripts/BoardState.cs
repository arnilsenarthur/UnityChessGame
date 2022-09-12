using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Main board class
    /// </summary>
    [Serializable]
    public class BoardState
    {
        
        public Vector2Int enPassantPosition = new Vector2Int(-1, -1);
        public char currentPlayer = 'w';

        private Piece[] _pieces = new Piece[64];
        public List<Piece> pieceList = new List<Piece>();
        private List<AttackLine> _currentAttackLines;

        public Action<GameObject> gameObject;

        public bool whiteCastleKingside = true;
        public bool whiteCastleQueenside = true;
        public bool blackCastleKingside = true;
        public bool blackCastleQueenside = true;

        public string startingFen = "";
        public int halfMoveCountSinceLastCapture = 0;
        public int fullMoveCount = 0;

        public List<string> moves = new List<string>();

        public Piece this[int f, int r] 
        {
            get => _pieces[f * 8 + r];
            set
            {
                Piece last = _pieces[f * 8 + r];

                if(last != null) pieceList.Remove(last);

                _pieces[f * 8 + r] = value;

                if(value == null)
                    return;

                value.Load();

                if(!pieceList.Contains(value))
                    pieceList.Add(value);
            } 
        }

        public string GetFEN()
        {
            string s = "";

            for(int i = 7; i >= 0; i --)
            {
                s += "/";
                int empty = 0;

                for(int j = 0; j <= 7; j ++)
                {
                    Piece piece = this[j,i];

                    if(piece != null)
                    {
                        if(empty > 0)
                        {
                            s += empty;
                            empty = 0;
                        }
                        
                        s += piece.isWhite ? Char.ToUpper(piece.type) : Char.ToLower(piece.type);
                    }
                    else
                        empty ++;
                }

                if(empty > 0)
                {
                    s += empty;
                    empty = 0;
                }
            }

            string castling = "";

            if(blackCastleKingside) castling += 'k';
            if(blackCastleQueenside) castling += 'q';
            if(whiteCastleKingside) castling += 'K';
            if(whiteCastleQueenside) castling += 'Q';

            string enpassant = "-";

            if(enPassantPosition.x != -1)
            {
                enpassant = new Pos(enPassantPosition.x, enPassantPosition.y).ToString();
            }

            s += $" {currentPlayer} {(castling.Length == 0 ? "-" : castling)} {enpassant} {halfMoveCountSinceLastCapture} {fullMoveCount}";

            return s.Substring(1);
        }

        /// <summary>
        /// Keep game working even after reload
        /// </summary>
        public void Reload()
        {
            _pieces = new Piece[64];
            foreach(Piece piece in pieceList)
                this[piece.position.x, piece.position.y] = piece;
        }

        /// <summary>
        /// Get the king for a current a player
        /// </summary>
        /// <param name="white"></param>
        /// <returns></returns>
        public Piece GetKing(bool white)
        {
            foreach(Piece piece in pieceList)
            {
                if(piece.type == 'k' && !white)
                    return piece;

                if(piece.type == 'K' && white)
                    return piece;
            }

            return null;
        }

        /// <summary>
        /// Generate state buffer for the current board state
        /// </summary>
        /// <param name="white"></param>
        /// <returns></returns>
        public BoardStateBuffer GenerateStateBuffer(bool white)
        {
            BoardStateBuffer buffer = new BoardStateBuffer();

            Piece king = GetKing(white);
            _currentAttackLines = king == null ? new List<AttackLine>() : GetAttackLines(king.position.x, king.position.y, !white);
        
            buffer.currentAttackLines = _currentAttackLines;

            foreach(Piece piece in pieceList)
            {
                if(piece.isWhite == white)
                {
                    List<Move> moves = new List<Move>();
                    moves.AddRange(GetMovesInternal(piece.position.x, piece.position.y));                  
                    buffer.moves.Add(piece.position, moves);
                    buffer.moveCount += moves.Count;
                }
            }

            int count = _currentAttackLines.Count((a) => a.isDirect); 
            buffer.inCheck = count > 0;
            buffer.inStaleMate = !buffer.inCheck && buffer.moveCount == 0;
            buffer.inCheckMate = buffer.inCheck && buffer.moveCount == 0;
            return buffer;
        }

        /// <summary>
        /// Get all the moves that a piece on [f,r] can perform (Checking for king) 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public IEnumerable<Move> GetMoves(int f,int r)
        {  
            Piece piece = this[f, r];

            if(piece == null || piece.populator == null)
                yield break;

            Piece king = GetKing(piece.isWhite);
            _currentAttackLines = king == null ? new List<AttackLine>() : GetAttackLines(king.position.x, king.position.y, !piece.isWhite);
        
            foreach(Move move in GetMovesInternal(f, r))
                yield return move;
        }
        
        /// <summary>
        /// Get all attack lines from a player against square [f, r] 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <param name="white"></param>
        /// <returns></returns>
        public List<AttackLine> GetAttackLines(int f, int r, bool white)
        {
            List<AttackLine> lines = new List<AttackLine>();

            foreach(Piece piece in pieceList)
            {
                if(piece.isWhite != white)
                    continue;

                piece.attackLineProviders?.Invoke(piece.position.x, piece.position.y, f, r, this, lines);
            }

            return lines;
        }

        /// <summary>
        /// Get all the moves that a piece on [f,r] can perform 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private IEnumerable<Move> GetMovesInternal(int f,int r)
        {
            Piece piece = this[f, r];

            int directCount = _currentAttackLines.Count((a) => a.isDirect);
 
            if(piece.type == 'k' || piece.type == 'K')
            {
                _pieces[f * 8 + r] = null;

                foreach(var v in piece.populator.PopulateMoves(piece, this))
                {
                    bool safe = true;
                    Vector2Int pos = new Vector2Int(v.to.x, v.to.z);

                    foreach(AttackLine line in GetAttackLines(v.to.x, v.to.z, piece.isBlack))
                    {
                        if(line.isDirect && line.from != pos)
                        {
                            safe = false;
                            break;
                        }
                    }
                    
                    if(safe)
                        yield return v;
                }

                _pieces[f * 8 + r] = piece;
                
                yield break;
            }

            if(directCount > 1) //check mate
            {
                yield break;
            }

            foreach(var v in piece.populator.PopulateMoves(piece, this))
            {
                if(directCount == 1)
                {
                    AttackLine direct = _currentAttackLines.Where((a) => a.isDirect).First();

                    if(!direct.CanDefend(v.to.x, v.to.z))
                        continue;
                }

                AttackLine line = _currentAttackLines.Where((a) => a.defender != null && a.defender == piece).FirstOrDefault();

                if(line != null && !line.CanDefend(v.to.x, v.to.z))
                    continue;
                
                //filter by attack lines
                yield return v;
            }
        }

        #region FEN
        public void LoadFEN(string notation)
        {
            foreach(var piece in pieceList)
                GameController.Destroy(piece.gameObject);

            pieceList.Clear();
            _pieces = new Piece[64];

            string[] fen = notation.Split(' ');

            if(fen.Length != 6)
                throw new Exception("Invalid FEN! Expected: <pieces> <nextmove> <castling> <en-passant> <halfmoves> <fullmoves>");

            #region Load Board Pieces
            string[] ranks = fen[0].Split('/');

            if(ranks.Length != 8)
                throw new Exception($"Invalid FEN! Expected 8 ranks but found {ranks.Length}");

            for(int rank = 0; rank < 8; rank ++)
            {
                int file = 0;
                foreach(char c in ranks[7 - rank])
                {
                    if(Char.IsNumber(c))
                        file += c - 48;
                    else
                    {
                        GameObject prefab = GameController.instance.GetPrefab(c);

                        if(prefab == null)
                            throw new Exception($"Invalid FEN! Invalid piece {c}");

                        GameObject go = GameController.Instantiate(prefab);
                        
                        go.transform.position = new Vector3(file, 0, rank);
                        this[file, rank] = new Piece{gameObject = go, type = c, position = new Vector2Int(file, rank)};  

                        file ++;
                    }
                }

                if(file != 8)
                    throw new Exception($"Invalid FEN! Expected 8 files on rank {rank + 1} but found {file}");
            } 
            #endregion

            #region Load Current Player
            if(fen[1].Length != 1)
                throw new Exception($"Invalid FEN! Invalid current player {fen[1]}");
            
            if(fen[1] == "w")
                currentPlayer = 'w';
            else if(fen[1] == "b")
                currentPlayer = 'b';
            else 
                throw new Exception($"Invalid FEN! Invalid current player {fen[1]}");
            #endregion

            #region Load Castle
            string castling = fen[2];

            whiteCastleKingside = castling.Contains('K');
            whiteCastleQueenside = castling.Contains('Q');
            blackCastleKingside = castling.Contains('k');
            blackCastleQueenside = castling.Contains('q');
            #endregion

            #region Load Current EnPassant
            string enPassantInfo = fen[3];

            if(enPassantInfo != "-")
            {
                int index = 0;
                Pos pos = SANGetPos(enPassantInfo.ToCharArray(), ref index);
                if(pos.file == '?' || pos.rank == '?')
                    throw new Exception($"Invalid FEN! Invalid enpassant position {fen[3]}");
                
                enPassantPosition = new Vector2Int(pos.x, pos.z);
            }
            else
                enPassantPosition = new Vector2Int(-1, -1);
            #endregion 

            startingFen = notation;
        }
        #endregion

        #region Utils
        public bool IsOnBounds(int f, int r)
        {
            return f > -1 && f < 8 && r > -1 && r < 8; 
        }

        public bool IsEmpty(int f, int r, BoardState state)
        {
            return state[f, r] == null || state[f, r].type == '?';
        }

        public bool IsMovable(Piece piece, int fb, int rb, BoardState state)
        {
            if(IsOnBounds(fb, rb))
            {
                if(IsEmpty(fb, rb, state))
                    return true;

                return state[fb, rb].isWhite != piece.isWhite;
            }

            return false;
        }

        public IEnumerable<Move> GetMoves(Piece piece, int df, int dr, char c, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            for(int i = 1; i < 8; i ++)
                if(state.IsMovable(piece, f + i * df, r + i * dr, state))
                {
                    yield return new MoveBuilder().Piece(c).From(f, r).To(f + i * df, r + i * dr).Capturing(!state.IsEmpty(f + i * df, r + i * dr, state)).Build();
                    if(!state.IsEmpty(f + i * df, r + i * dr, state)) break;
                }
                else break;
        }
        #endregion

        #region UCI
        public Move GetUCIMove(string san)
        {
            char[] chars = san.ToCharArray();
            int index = 0;

            Move move = new Move();
            move.piece = '?';
            move.from = SANGetPos(chars, ref index); 
            move.to = SANGetPos(chars, ref index); 

            if(chars.Length == 5)
            {
                chars[4] = Char.ToUpper(chars[4]);
                move.isPromotion = true;
                move.promoteTo = SANGetPieceType(chars,ref index);
            }

            return move;
        }
        #endregion

        #region SAN
        /// <summary>
        /// Create a Move object from a SAN notation
        /// 
        /// <piece-type?> <from?> <x:capture?> <to> <=:promotion?> <promotiontype?> <+:check?> <#:mate?>
        /// <O-O> <+:check?> <#:mate?>
        /// <O-O-O> <+:check?> <#:mate?>
        /// 
        /// Ex.:
        /// 
        /// c3 (Pawn goes to c3)
        /// Qxd5+ (Queen goes to d5, capturing, then checking)
        ///
        /// </summary>
        /// <param name="san"></param>
        /// <returns></returns>
        public Move GetSANMove(string san)
        {
            char[] chars = san.ToCharArray();
            int index = 0;

            Move move = new Move();

            move.isCastling = san.StartsWith("O-O"); 
            move.isCastlingQueenside = move.isCastling && san.StartsWith("O-O-O");

            move.piece = SANGetPieceType(chars, ref index);
            Pos pos1 = SANGetPos(chars, ref index); 
            move.isCapture = SANGetChar(chars, ref index, 'x');
            Pos pos2 = SANGetPos(chars, ref index);
            move.isPromotion = SANGetPromotion(chars, ref index, out char promotion);
            move.promoteTo = promotion;
            move.isCheck = SANGetChar(chars, ref index, '+');
            move.isCheck = move.isCheck || (move.isCheckMate = SANGetChar(chars, ref index, '#')); 

            #region Switch positions in case of disambiguation
            move.to = pos1;
            move.from = Pos.EMPTY;

            if(!pos2.isEmpty)
            {
                move.to = pos2;
                move.from = pos1;
            }
            #endregion

            return move;
        }

        /// <summary>
        /// Get piece type from SAN notation
        
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private char SANGetPieceType(char[] chars, ref int index)
        {
            if(chars.Length <= index)
                return '?';

            index ++;
            switch(chars[index - 1])
            {
                case 'K': return 'K';
                case 'Q': return 'Q';
                case 'B': return 'B';
                case 'N': return 'N';
                case 'R': return 'R';
                case 'P': return 'P';
            }

            index --;
            return 'P';
        }

        /// <summary>
        /// Get board square from SAN notation
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Pos SANGetPos(char[] chars, ref int index)
        {
            Pos pos = Pos.EMPTY;
            
            if(chars.Length <= index)
                return pos;

            if(chars[index] > 96 && chars[index] < 105)
            {
                pos.file = chars[index];
                index ++;
            }

            if(chars.Length <= index)
                return pos;

            if(chars[index] > 47 && chars[index] < 58)
            {
                pos.rank = chars[index];
                index ++;
            }  

            return pos;
        }

        /// <summary>
        /// Check if SAN notation contains a char:
        /// 
        ///     + - check
        ///     # - mate
        ///     x - capture
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="index"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool SANGetChar(char[] chars, ref int index, char c)
        {
            if(chars.Length <= index)
                return false;

            if(chars[index] == c)
            {
                index ++;
                return true;
            }

            return false;
        }   

        /// <summary>
        /// Get if SAN move is a promotion, and what is the target piece type
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="index"></param>
        /// <param name="promotion"></param>
        /// <returns></returns>
        private bool SANGetPromotion(char[] chars, ref int index, out char promotion)
        {
            promotion = '?';
            if(chars.Length <= index)
                return false;

            if(chars[index] != '=')
                return false;

            index ++;
            promotion = SANGetPieceType(chars, ref index);
            return true;
        }
        #endregion
    }
}