
using System.Collections;
using System.Text;

public enum Player {
    WHITE,
    BLACK
}

public class ChessEngine {
    public Board board = new();
    // If a player moved a pawn 2 spaces, and it can be taken en passant, this will be set to the file of the pawn moved.
    // This gets reset to -1 when there is no pawn that can be taken en passant.
    public int enPassantFile = -1;
    public Player currentPlayer = Player.WHITE;
    public bool MovePiece(Coord from, Coord to) {
        if (IsValidMove(from, to, currentPlayer)) {
            // Check for castling, en passant first

            // If capture, do special thing
            var piece = board.GetPieceAtPosition(from);

            board.SetPieceAtPosition(to, piece);
            board.SetPieceAtPosition(from, '.');

            // En Passant

            // 1. When capturing pawn that advanced forward
            if (
                enPassantFile > 0 && 
                (piece == 'p' || piece == 'P') // If pawn..
                && Math.Abs(to.file - from.file) == 1 // That captured diagonally..
                && Math.Abs(to.rank - from.rank) == 1
                && enPassantFile == to.file // On a file where a pawn just moved en passant..
                && ((currentPlayer == Player.WHITE && to.rank == 6) // For white, pawn needs to go to 6th rank
                || (currentPlayer == Player.BLACK && to.rank == 3)) // For black, pawn needs to go to 3rd rank
            ) {
                board.SetPieceAtPosition(new Coord(currentPlayer == Player.WHITE ? 5 : 4, enPassantFile), '.'); // Capture the pawn that advanced
            }

            enPassantFile = -1;
            // 1. When advancing pawn forward
            if (
                (piece == 'p' || piece == 'P') // If pawn
                && to.file == from.file // .. that moved vertically
                && Math.Abs(to.rank - from.rank) == 2 // .. 2 spaces
            ) {
                enPassantFile = to.file; 
            }

            

            if(currentPlayer == Player.WHITE) {
                currentPlayer = Player.BLACK;
            } else {
                currentPlayer = Player.WHITE;
            }

            return true;
        }
        return false;
    }

    public bool IsOwnPiece(Coord coord, Player player) {
        var piece =  board.GetPieceAtPosition(coord);
        if (player == Player.WHITE) { 
            return piece >= 'a' && piece <= 'z';
        }
        else { 
            return piece >= 'A' && piece <= 'Z';
        }
    }

    private bool IsSamePieceColor(char pieceA, char pieceB) {
        return false;
    }


    public Coord? FindKing(Board boardToUse, Player player) {
        for (int r = 1; r <= 8; r++) {
            for (int f = 1; f <= 8; f++) {
                var coord = new Coord(r, f);
                var piece = boardToUse.GetPieceAtPosition(coord);
                if ((piece == 'k' || piece == 'K') && IsOwnPiece(coord, player)) {
                    return coord;
                }
            }
        }
        return null; // There isn't a king on the board???
    }

    public bool InCheck(Board boardToUse, Player player) {
        // Find where the player's king is
        Coord? kingPos = FindKing(boardToUse, player);
        if (kingPos == null) return false; // Can't be in check if you dont have a king lol
        // Check every square
        for (int r = 1; r <= 8; r++) {
            for (int f = 1; f <= 8; f++) {
                var coord = new Coord(r, f);
                var piece = boardToUse.GetPieceAtPosition(coord);

                // Ignore own player's pieces and empty squares
                if (IsOwnPiece(coord, player) || piece == '.') continue;

                var canTake = IsValidMove(coord, kingPos, player == Player.WHITE ? Player.BLACK : Player.WHITE);
                
                // If the opponent can capture the king with this piece on the next move provided the current player does nothing, the king is in check.
                if (canTake) return true;
            }
        }
        return false;
    }
    public bool WillPutKingInCheck(Coord from, Coord to, Player player) {
        Board newBoard = new Board(board);
        var piece = newBoard.GetPieceAtPosition(from);
        newBoard.SetPieceAtPosition(to, piece);
        newBoard.SetPieceAtPosition(from, '.');

        return InCheck(newBoard, player);


    }
    public bool IsValidMove(Coord from, Coord to, Player forPlayer) {

        // Cannot move piece to same square
        if (from == to) return false;

        // Cannot move another player's piece
        if (!IsOwnPiece(from, forPlayer)) return false;

        // Check if move will put/leave king in check! If so, not valid move!
        // if (WillPutKingInCheck(from, to, forPlayer)) return false;

        char piece = board.GetPieceAtPosition(from);


        // Rules for moving major pieces
        // 1. King
        if (piece == 'k' || piece == 'K') {
            // TODO Check for castling
            // Can only move 1 square
            if (Math.Abs(from.rank - to.rank) <= 1 && Math.Abs(from.file - to.file) <= 1) return true;
            return false;
        }
        // 2. Queen
        else if (piece == 'q' || piece == 'Q') {
            // Must be along diagonal or straight
            if (!IsStraightMove(from, to) && !IsDiagonalMove(from, to)) return false;

            return true;
        }

        // 3. Rook
        else if (piece == 'r' || piece == 'R') {
            if (!IsStraightMove(from, to)) return false;

            return true;
        }

        // 4. Knight
        else if (piece == 'n' || piece == 'N') {
            if (!IsKnightMove(from, to)) return false;

            return true;
        }

        // 5. Bishop
        else if (piece == 'b' || piece == 'B') {
            if (!IsDiagonalMove(from, to)) return false;

            return true;
        }

        // 6. Pawns
        else if (piece == 'p' || piece == 'P') {
            var isWhite = forPlayer == Player.WHITE;

            var pawnRank = isWhite ? 2 : 7;
            var secondRank = isWhite ? 3 : 6;
            var thirdRank = isWhite ? 4 : 5;
            

            // Moving pawn vertically
            if (to.file == from.file) {
                // If first move
                if (from.rank == pawnRank) { 
                    // Allow moving 2 spaces provided that both of the spaces in front are clear
                    if (to.rank == thirdRank) {
                        if (board.GetPieceAtPosition(new Coord(secondRank, from.file)) == '.'
                        && board.GetPieceAtPosition(to) == '.') return true;
                        // TODO add logic to prepare en passant
                    }
                }
                // Just advancing normally along the board
                if ((isWhite && to.rank == from.rank + 1) || (!isWhite && to.rank == from.rank - 1)) {
                        if (board.GetPieceAtPosition(to) == '.') return true;
                }
            } else if (Math.Abs(to.file - from.file) == 1) {
                // Capturing diagonally

                // If white, must be increasing rank to capture
                if (isWhite && to.rank - from.rank != 1) return false;
                // If black, must be decreasing rank to capture
                if (!isWhite && to.rank - from.rank != -1) return false; 

                // Must be landing on enemy piece
                if (!IsOwnPiece(to, forPlayer) && board.GetPieceAtPosition(to) != '.') return true;

                // ... or Capturing en passant
                if (to.file == enPassantFile) {
                    // If white, must land on rank 6
                    if (isWhite && to.rank == 6) return true;
                    // If black, must land on rank 3
                    if (!isWhite && to.rank == 3) return true;
                }
                return false;
            }
            return false;
        }

        return false;
    }

    private bool IsKnightMove(Coord from, Coord to) {
            var rankDiff = Math.Abs(to.rank - from.rank);
            var fileDiff = Math.Abs(to.file - from.file);
            if ((rankDiff == 2 && fileDiff == 1) || (rankDiff == 1 && fileDiff == 2)) return true;

            return false;
    }

    private bool IsDiagonalMove(Coord from, Coord to) {
            var rankDiff = Math.Abs(to.rank - from.rank);
            var fileDiff = Math.Abs(to.file - from.file);

            if (rankDiff == fileDiff) return true;
            
            return false;
    }

    private bool IsStraightMove(Coord from, Coord to) {
            var rankDiff = Math.Abs(to.rank - from.rank);
            var fileDiff = Math.Abs(to.file - from.file);

            if (rankDiff == 0 || fileDiff == 0) return true;

            return false;
    }

}


[Serializable]
public record Coord(int rank, int file);

public class Board {

    public char[][] ranks {get; set;} = {
        "rnbqkbnr".ToCharArray(), // rank 1 (white)
        "pppppppp".ToCharArray(),
        "........".ToCharArray(),
        "........".ToCharArray(),
        "........".ToCharArray(),
        "........".ToCharArray(),
        "PPPPPPPP".ToCharArray(),
        "RNBQKBNR".ToCharArray(), // rank 8 (black)
    };

    public Board() {
    }
    // Copies the piece data from a previous board position
    public Board(Board previousBoard) {
        for (int i = 0; i < 8; i++)
            Array.Copy(previousBoard.ranks[i], ranks[i], 8);
    }

    public char GetPieceAtPosition(Coord coord) {
        return ranks[coord.rank-1][coord.file-1];
    }

    // coord: coordinates of square in chess notation, e.g. 'd4' or 'e2'
    public char GetPieceAtPosition(string coord) {
        int file = coord[0] - 'a' - 1;
        int rank = coord[1] - '0';
        return GetPieceAtPosition(new Coord(rank, file));
    }

    public void SetPieceAtPosition(Coord coord, char piece) {
        ranks[coord.rank-1][coord.file-1] = piece;
    }

    public String GetBoardStr() {
        StringBuilder sb = new();
        foreach (var rank in ranks.Reverse()) {
            foreach(char piece in rank) {
                sb.Append(piece);
            }
            sb.AppendLine();
        }

        return sb.ToString();

    }
    
}

public class PlayerInfo {
    public bool isWhite;
    // If given the opportunity, can the player castle kingside?
    public bool kingsideCastlingRights = true;
    // If given the opportunity, can the player castle queenside?
    public bool queensideCastlingRights = true;


    public PlayerInfo(bool isWhite) {
        this.isWhite = isWhite;
    }

}