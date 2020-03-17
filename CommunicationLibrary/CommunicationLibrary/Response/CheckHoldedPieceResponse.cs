﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Response
{
    public class CheckHoldedPieceResponse : MessagePayload
    {
        public bool? Sham { get; set; }

        public override bool ValidateMessage()
        {
            if (Sham == null)
                return false;
            return true;
        }
    }
}
