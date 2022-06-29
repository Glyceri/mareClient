﻿using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace MareSynchronos.WebAPI.Utils;

public class ForeverRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(5);
    }
}