using CommunicationLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Response
{
    public class MoveResponse : MessagePayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("madeMove")]
        public bool? MadeMove { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("currentPosition")]
        public Position CurrentPosition { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("closestPiece")]
        public int? ClosestPiece { get; set; }

        public override bool ValidateMessage()
        {
            if (MadeMove == null)
                return false;
            if (CurrentPosition.X == null || CurrentPosition.Y == null)
                return false;
            if (ClosestPiece == null || ClosestPiece < 0)
                return false;
            return true;
        }
    }
}
