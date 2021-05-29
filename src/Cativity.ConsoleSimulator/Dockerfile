FROM mcr.microsoft.com/dotnet/sdk:5.0
WORKDIR /build
COPY Cativity.ConsoleSimulator.fsproj ./
RUN dotnet restore 
COPY . .
RUN dotnet publish -o /publish 
WORKDIR /publish 
ENTRYPOINT ["dotnet", "Cativity.ConsoleSimulator.dll"]