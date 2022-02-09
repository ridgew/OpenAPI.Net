﻿using System;

namespace OpenAPI.Net.Exceptions
{
    /// <summary>
    /// The exception that is thrown while reading network stream in OpenClient
    /// The innerException is the real exception that was thrown.
    /// </summary>
    public class ReceiveException : Exception
    {
        public ReceiveException(Exception innerException) : base("An exception occurred while reading from stream", innerException)
        {
        }
    }
}