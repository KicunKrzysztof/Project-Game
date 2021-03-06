using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Error
{
    public class PickPieceError : MessagePayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("errorSubtype")]
        public string ErrorSubtype { get; set; }

        public override bool ValidateMessage()
        {
            if (ErrorSubtype == null || (ErrorSubtype != "NothingThere" && ErrorSubtype != "Other"))
                return false;
            return true;
        }
    }
}
