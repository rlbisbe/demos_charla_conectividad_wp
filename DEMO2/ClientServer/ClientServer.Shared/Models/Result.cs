using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServer.Models
{
    public enum ResultType
    {
        Text = 0,
        Image = 1
    }

    public class Result
    {
        public ResultType ResultType { get; set; }
        public byte[] Value { get; set; }
    }
}
