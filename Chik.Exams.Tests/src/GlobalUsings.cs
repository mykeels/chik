global using NUnit.Framework;
global using Chik.Exams;
global using Chik.Exams.Data;
global using Moq;
global using Microsoft.Extensions.Logging;
global using ZiggyCreatures.Caching.Fusion;

// Disable parallel test execution to prevent database race conditions
[assembly: NonParallelizable]