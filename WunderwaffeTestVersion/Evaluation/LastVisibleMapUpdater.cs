///*
//* this code was auto-converted from a java project.
//*/

//using warlight.ai.wunderwaffe.bot;
//using warlight.shared.ai.wunderwaffe.bot;

//using warlight.shared.ai.wunderwaffe.move;

//namespace warlight.shared.ai.wunderwaffe.evaluation
//{
//    public class lastvisiblemapupdater
//    {
//        public botmain botstate;
//        public lastvisiblemapupdater(botmain state)
//        {
//            this.botstate = state;
//        }
        
//        public void storeopponentdeployment()
//        {
//            var lastvisiblemap = botstate.lastvisiblemap;
//            foreach (var opponentterritory in lastvisiblemap.allopponentterritories)
//            {
//                var armiesdeployed = botstate.historytracker.getopponentdeployment(opponentterritory.ownerplayerid);
//                if (armiesdeployed > 0)
//                    movescommitter.committplacearmiesmove(new botorderdeploy(opponentterritory.ownerplayerid, opponentterritory, armiesdeployed));
//            }
//        }
//    }
//}
