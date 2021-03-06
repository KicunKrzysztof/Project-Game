using Agent.Board;
using CommunicationLibrary;
using CommunicationLibrary.Error;
using CommunicationLibrary.Model;
using CommunicationLibrary.Response;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Agent.Strategies
{
    public abstract class Strategy : IStrategy
    {
        virtual public AgentBoard Board { get; private set; }
        public Strategy() { }
        public Strategy(int width, int height, string teamId, int goalAreaSize)
        {
            Board = new AgentBoard(width, height, teamId, goalAreaSize);
        }
        public abstract Message MakeDecision(AgentInfo agent);
        public virtual void UpdateMap(Message message, Point position)
        {
            switch (message.MessageId)
            {
                case MessageType.CheckHoldedPieceResponse:
                    CheckHoldedPieceResponseHandler((CheckHoldedPieceResponse)message.GetPayload());
                    break;
                case MessageType.DiscoveryResponse:
                    DiscoveryResponseHandler((DiscoveryResponse)message.GetPayload(), position);
                    break;
                case MessageType.ExchangeInformationGMResponse:
                    ExchangeInformationResponseHandler((ExchangeInformationGMResponse)message.GetPayload());
                    break;
                case MessageType.DestroyPieceResponse:
                    DestroyPieceResponseHandler((DestroyPieceResponse)message.GetPayload());
                    break;
                case MessageType.MoveResponse:
                    MoveResponseHandler((MoveResponse)message.GetPayload());
                    break;
                case MessageType.PickPieceResponse:
                    PickPieceResponseHandler((PickPieceResponse)message.GetPayload(), position);
                    break;
                case MessageType.PutPieceResponse:
                    PutPieceResponseHandler((PutPieceResponse)message.GetPayload(), position);
                    break;
                case MessageType.MoveError:
                    MoveErrorResponseHandler((MoveError)message.GetPayload());
                    break;
                case MessageType.PickPieceError:
                    PickPieceErrorResponseHandler((PickPieceError)message.GetPayload(), position);
                    break;
                case MessageType.PutPieceError:
                    PutPieceErrorResponseHandler((PutPieceError)message.GetPayload());
                    break;
                case MessageType.PenaltyNotWaitedError:
                    PenaltyNotWaitedErrorResponseHandler((PenaltyNotWaitedError)message.GetPayload());
                    break;
                case MessageType.NotDefinedError:
                    NotDefinedResponseHandler((NotDefinedError)message.GetPayload());
                    break;
            }
        }
        public Func<int, int, int, int, int> updateDistanse = (dist, lastUpdate, curDist, curLastUpdate) =>
        {
            return lastUpdate > curLastUpdate ? curDist : dist;
        };
        virtual protected void CheckHoldedPieceResponseHandler(CheckHoldedPieceResponse checkHoldedPieceResponse) { }
        virtual protected void DiscoveryResponseHandler(DiscoveryResponse discoveryResponse, Point position)
        {
            Func<int, int, bool> insideBoard =
                (x, y) => x >= 0 && y >= 0 && x < Board.Board.GetLength(0) && y < Board.Board.GetLength(1);
            Board.Board[position.X, position.Y].DistToPiece = discoveryResponse.DistanceFromCurrent;
            if (insideBoard(position.X - 1, position.Y + 1))
                Board.Board[position.X - 1, position.Y + 1].DistToPiece = discoveryResponse.DistanceNW;
            if (insideBoard(position.X, position.Y + 1))
                Board.Board[position.X, position.Y + 1].DistToPiece = discoveryResponse.DistanceN;
            if (insideBoard(position.X + 1, position.Y + 1))
                Board.Board[position.X + 1, position.Y + 1].DistToPiece = discoveryResponse.DistanceNE;
            if (insideBoard(position.X - 1, position.Y))
                Board.Board[position.X - 1, position.Y].DistToPiece = discoveryResponse.DistanceW;
            if (insideBoard(position.X + 1, position.Y))
                Board.Board[position.X + 1, position.Y].DistToPiece = discoveryResponse.DistanceE;
            if (insideBoard(position.X - 1, position.Y - 1))
                Board.Board[position.X - 1, position.Y - 1].DistToPiece = discoveryResponse.DistanceSW;
            if (insideBoard(position.X, position.Y - 1))
                Board.Board[position.X, position.Y - 1].DistToPiece = discoveryResponse.DistanceS;
            if (insideBoard(position.X + 1, position.Y - 1))
                Board.Board[position.X + 1, position.Y - 1].DistToPiece = discoveryResponse.DistanceSE;
        }
        virtual protected void DestroyPieceResponseHandler(DestroyPieceResponse moveError) { }
        virtual protected void ExchangeInformationResponseHandler(ExchangeInformationGMResponse exchangeInformationResponse)
        {
            List<string> red = new List<string>(exchangeInformationResponse.RedTeamGoalAreaInformations);
            List<string> blue = new List<string>(exchangeInformationResponse.BlueTeamGoalAreaInformations);
            Board.UpdateGoalInfo(red.Count >blue.Count ? red : blue);
        }
        virtual protected void MoveResponseHandler(MoveResponse moveResponse)
        {
            Board.Board[moveResponse.CurrentPosition.X.Value, moveResponse.CurrentPosition.Y.Value].DistToPiece = moveResponse.ClosestPiece.Value;
        }
        virtual protected void NotDefinedResponseHandler(NotDefinedError notDefinedError) { }
        virtual protected void MoveErrorResponseHandler(MoveError moveError) { }
        virtual protected void PickPieceErrorResponseHandler(PickPieceError pieceError, Point position)
        {
            Board.Board[position.X, position.Y].DistToPiece = Int32.MaxValue;
        }
        virtual protected void PickPieceResponseHandler(PickPieceResponse pickPieceRespone, Point position)
        {
            Board.Board[position.X, position.Y].DistToPiece = int.MaxValue;
            if (position.Y != Board.Board.GetLength(1) - 1)
                Board.Board[position.X, position.Y + 1].DistToPiece = int.MaxValue;

            if (position.Y != 0)
                Board.Board[position.X, position.Y - 1].DistToPiece = int.MaxValue;

            if (position.Y != Board.Board.GetLength(1) - 1 && position.X != Board.Board.GetLength(0) - 1)
                Board.Board[position.X + 1, position.Y + 1].DistToPiece = int.MaxValue;

            if (position.X != 0)
                Board.Board[position.X - 1, position.Y].DistToPiece = int.MaxValue;

            if (position.X != Board.Board.GetLength(0) - 1)
                Board.Board[position.X + 1, position.Y].DistToPiece = int.MaxValue;

            if (position.Y != 0 && position.X != 0)
                Board.Board[position.X - 1, position.Y - 1].DistToPiece = int.MaxValue;

            if (position.Y != 0 && position.X != Board.Board.GetLength(0) - 1)
                Board.Board[position.X + 1, position.Y - 1].DistToPiece = int.MaxValue;
        }
        virtual protected void PutPieceErrorResponseHandler(PutPieceError putPieceError) { }
        virtual protected void PutPieceResponseHandler(PutPieceResponse putPieceRespone, Point position)
        {
            switch (putPieceRespone.PutResult)
            {
                case PutResultEnum.NormalOnGoalField:
                    Board.Board[position.X, position.Y].goalInfo = GoalInfo.DiscoveredGoal;
                    break;
                case PutResultEnum.NormalOnNonGoalField:
                    Board.Board[position.X, position.Y].goalInfo = GoalInfo.DiscoveredNotGoal;
                    break;
                case PutResultEnum.TaskField:
                case PutResultEnum.ShamOnGoalArea:
                    //TaskField- odłożynie na pole które nie jeste goalem (środek planszy)
                    //ShamOnGoalArea-nie wiemy czy pole pod było prawdzym goalem czy nie
                    break;
            }
        }
        virtual protected void PenaltyNotWaitedErrorResponseHandler(PenaltyNotWaitedError penaltyNotWaitedError)
        {

        }
    }
}
