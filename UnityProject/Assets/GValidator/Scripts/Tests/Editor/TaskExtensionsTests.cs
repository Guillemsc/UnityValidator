using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GValidator.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GValidator.Tests
{
    public sealed class TaskExtensionsTests
    {
        [Test]
        public void FireAndForget_LogsTaskExceptions()
        {
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: fire-and-forget failure"));
            Task task = Task.FromException(
                new InvalidOperationException("fire-and-forget failure"));

            task.FireAndForget();

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void FireAndForget_AllowsSuccessfulTasksToCompleteWithoutLogs()
        {
            Task.CompletedTask.FireAndForget();

            LogAssert.NoUnexpectedReceived();
        }
    }
}
