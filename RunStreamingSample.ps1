
# Start Orleans SILO
powershell -NoProfile -Command "E:\Source\Samples\iWorkTech.Orleans\iWorkTech.Orleans.SiloHost dotnet run"

# Start ASP.NET Core
powershell -NoProfile -Command "E:\Source\Samples\iWorkTech.Orleans\iWorkTech.Orleans.Web.Core dotnet run"

# Start SignalR Client
powershell -NoProfile -Command "E:\Source\Samples\iWorkTech.Orleans\iWorkTech.SignalR.Console.Streaming.Client dotnet run"
