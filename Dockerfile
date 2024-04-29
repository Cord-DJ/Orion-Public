FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

COPY output .

RUN chmod +x Cord.Server
ENTRYPOINT ["./Cord.Server"]
