﻿using CommunicationLibrary;
using CommunicationLibrary.Request;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using FibonacciHeap;
using Agent.AgentBoard;

namespace Agent
{
    public class Agent : IDisposable
    {
        public AgentConfiguration _configuration { get; set; }
        private StreamMessageSenderReceiver _communicator;
        private Queue<Message> _queue = new Queue<Message>();
        private TcpClient _client;
        public AgentInfo agentInfo;
        //FibonacciHeap<Message, int> _queue = new FibonacciHeap<Message, int>(1);


        public Agent(AgentConfiguration configuration)
        {
            this._configuration = configuration;
            _client = new TcpClient(_configuration.CsIp, _configuration.CsPort);
            NetworkStream stream = _client.GetStream();
            this._communicator = new StreamMessageSenderReceiver(stream, new Parser());
            if (_configuration.Strategy == 1)
            {
                AgentBoardInfo boardInfo = new AgentBoardInfo();
                this.agentInfo = new AgentInfo(new SampleStrategy(), true, boardInfo, (0, 0)); //TO DO: position and other informations from GM message.
            }
        }

        public void StartListening()
        {
            ThreadPool.SetMaxThreads(2, 1);
            _communicator.Send(new Message<JoinGameRequest>() { MessagePayload = new JoinGameRequest { TeamId = "blue" } });
            _communicator.StartReceiving(this.AddToQueue);
        }

        public void StartSending(object message)
        {
            Random r = new Random();

            Console.ReadKey();
            var p = r.Next() % 2;
            if (p == 0)
                _communicator.Send(new Message<JoinGameRequest>() { MessagePayload = new JoinGameRequest { TeamId = "blue" } });
            else
                _communicator.Send(new Message<PickPieceRequest>() { MessagePayload = new PickPieceRequest() });
        }

        private void AddToQueue(Message message)
        {
            Console.WriteLine("Got message: " + message.MessageId);
            _queue.Enqueue(message);
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.StartSending));
        }

        public void Dispose()
        {
            _client.Dispose();
            _communicator.Dispose();
        }
    }
}

