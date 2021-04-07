FROM debian:stable-slim

RUN apt-get update && apt-get install ca-certificates gnupg curl -y
RUN curl https://download.mono-project.com/repo/xamarin.gpg | apt-key add -

RUN echo "deb https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list
RUN apt-get update -qq \
	&& apt-get install -y mono-mcs mono-devel ca-certificates-mono

ADD . .

RUN mcs src/LingotekDemo.cs -o:LingotekDemo.exe /reference:System.Net.Http.dll \
		/reference:System.Web.Extensions.dll /reference:System.Net.Http.Formatting.dll

RUN mv src/LingotekDemo.exe /usr/local/bin/LingotekDemo.exe

ENTRYPOINT mono /usr/local/bin/LingotekDemo.exe && cp -a /usr/local/bin/LingotekDemo.exe /output
