namespace WarLight.AI.Wunderwaffe.Move
{
    public class SameMovesJoiner
    {
        //
        // public static void joinSameMoves(Moves moves) {
        // List<AttackTransferMove> ourAttacks = moves.attackTransferMoves;
        // List<AttackTransferMove> joinedAttacks = joinSameMoves(ourAttacks);
        // moves.attackTransferMoves = joinedAttacks;
        // }
        //
        // private static List<AttackTransferMove>
        // joinSameMoves(List<AttackTransferMove> in) {
        // List<AttackTransferMove> out = new ArrayList<AttackTransferMove>();
        // List<AttackTransferMove> illegalMoves = new
        // ArrayList<AttackTransferMove>();
        // // Step 1
        // for (int i = 0; i < in.size(); i++) {
        // AttackTransferMove movei = in.get(i);
        // for (int j = i + 1; j < in.size(); j++) {
        // AttackTransferMove movej = in.get(j);
        // if (movei.FromTerritory == movej.FromTerritory)
        // && movei.ToTerritory == movej.ToTerritory)) {
        // if (!illegalMoves.contains(movej)) {
        // illegalMoves.add(movej);
        // }
        // }
        // }
        // }
        // // Step two
        // for (int i = 0; i < in.size(); i++) {
        // AttackTransferMove movei = in.get(i);
        // if (!illegalMoves.contains(movei)) {
        // for (int j = 0; j < illegalMoves.size(); j++) {
        // AttackTransferMove movej = illegalMoves.get(j);
        // if (movei.FromTerritory == movej.FromTerritory)
        // && movei.ToTerritory == movej.ToTerritory)) {
        // movei.Armies = (movei.Armies + movej.Armies);
        // if (!movej.Message == "")) {
        // movei.setMessage(movej.Message);
        // }
        // }
        //
        // }
        // out.add(movei);
        // }
        // }
        // return out;
        // }
    }
}
