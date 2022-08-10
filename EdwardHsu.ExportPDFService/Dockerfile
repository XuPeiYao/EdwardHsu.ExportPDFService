#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

ENV ASPNETCORE_URLS=http://+:3000
WORKDIR /app
EXPOSE 3000

RUN apt update
RUN apt install -y unzip 
RUN apt install -y fontconfig
RUN apt install -y software-properties-common
RUN apt install -y chromium
RUN apt install -y wget

RUN mkdir /font-tmp
RUN cd /font-tmp
RUN wget https://fonts.google.com/download?family=Noto%20Sans%20TC -O Noto.zip
RUN unzip Noto.zip -d Noto
RUN mv Noto /usr/local/share/fonts
RUN chown root:staff /usr/local/share/fonts/Noto -R
RUN chmod 644 /usr/local/share/fonts/Noto/*
RUN chmod 755 /usr/local/share/fonts/Noto
RUN fc-cache -fv

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EdwardHsu.ExportPDFService/EdwardHsu.ExportPDFService.csproj", "EdwardHsu.ExportPDFService/"]
RUN dotnet restore "EdwardHsu.ExportPDFService/EdwardHsu.ExportPDFService.csproj"
COPY . .
WORKDIR "/src/EdwardHsu.ExportPDFService"
RUN dotnet build "EdwardHsu.ExportPDFService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EdwardHsu.ExportPDFService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "EdwardHsu.ExportPDFService.dll"]