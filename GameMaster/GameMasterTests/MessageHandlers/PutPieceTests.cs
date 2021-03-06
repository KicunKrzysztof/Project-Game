using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationLibrary.Request;
using CommunicationLibrary.Response;
using CommunicationLibrary.Error;
using CommunicationLibrary;
using CommunicationLibrary.Model;
using GameMaster.Game;
using GameMaster.MessageHandlers;
using GameMaster.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using FluentAssertions;

namespace GameMasterTests.MessageHandlers.Tests
{
    [TestClass()]
    public class PutPieceTests
    {
        private GMConfiguration config = new GMConfiguration()
        {
            BoardX = 40,
            BoardY = 40,
            CsIP = "127.0.0.1",
            CsPort = 8080,
            MovePenalty = 1500,
            DiscoveryPenalty = 700,
            PutPenalty = 500,
            CheckForShamPenalty = 700,
            InformationExchangePenalty = 1000,
            GoalAreaHeight = 5,
            NumberOfGoals = 5,
            NumberOfPieces = 10,
            ShamPieceProbability = 20
        };
        [TestMethod()]
        public void TestPlayerHaveNoPiece()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 5, y: 5, id: 1, team: Team.Red) };
            var map = new Map(players: players);
            map.GetPlayerById(1).Holding = null;
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest() { }
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceError> expectedResult = new Message<PutPieceError>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceError()
                {
                    ErrorSubtype = "AgentNotHolding"
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
        [TestMethod()]
        public void TestRealPieceOnTaskField()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 5, y: 5, 1, Team.Red) };
            var map = new Map(players: players);
            map.GetPlayerById(1).Holding = new Piece();
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest(){}
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceResponse> expectedResult = new Message<PutPieceResponse>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceResponse()
                {
                    PutResult = PutResultEnum.TaskField
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
        [TestMethod()]
        public void TestShamPieceOnTaskField()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 5, y: 5, 1, Team.Red) };
            var map = new Map(players: players);
            map.GetPlayerById(1).Holding = new ShamPiece();
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest() { }
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceResponse> expectedResult = new Message<PutPieceResponse>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceResponse()
                {
                    PutResult = PutResultEnum.TaskField
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
        [TestMethod()]
        public void TestRealPieceOnGoalAreaField()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 0, y: 0, 1, Team.Red) };
            var map = new Map(players: players);
            map.GetPlayerById(1).Holding = new Piece();
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest() { }
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceResponse> expectedResult = new Message<PutPieceResponse>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceResponse()
                {
                    PutResult = PutResultEnum.NormalOnNonGoalField
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
        [TestMethod()]
        public void TestShamPieceOnGoalAreaField()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 0, y: 0, 1, Team.Red) };
            var map = new Map(players: players);
            map.GetPlayerById(1).Holding = new ShamPiece();
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest() { }
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceResponse> expectedResult = new Message<PutPieceResponse>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceResponse()
                {
                    PutResult = PutResultEnum.ShamOnGoalArea
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
        [TestMethod()]
        public void TestRealPieceOnGoalField()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 0, y: 0, 1, Team.Red) };
            var goals = new List<(int x, int y)>() { (x: 0, y: 0) };
            var map = new Map(players: players, goalFields: goals);
            map.GetPlayerById(1).Holding = new Piece();
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest() { }
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceResponse> expectedResult = new Message<PutPieceResponse>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceResponse()
                {
                    PutResult = PutResultEnum.NormalOnGoalField
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
        [TestMethod()]
        public void TestShamPieceOnGoalField()
        {
            //given
            var players = new List<(int x, int y, int id, Team team)>() { (x: 0, y: 0, 1, Team.Red) };
            var goals = new List<(int x, int y)>() { (x: 0, y: 0) };
            var map = new Map(players: players, goalFields: goals);
            map.GetPlayerById(1).Holding = new ShamPiece();
            var message = new Message<PutPieceRequest>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceRequest() { }
            };
            var moveHandler = new PutPieceRequestHandler();
            Message<PutPieceResponse> expectedResult = new Message<PutPieceResponse>()
            {
                AgentId = 1,
                MessagePayload = new PutPieceResponse()
                {
                    PutResult = PutResultEnum.ShamOnGoalArea
                }
            };
            //when
            Message response = moveHandler.ProcessRequest(map, message, config);

            //then
            response.Should().BeEquivalentTo(expectedResult);
        }
    }
}
