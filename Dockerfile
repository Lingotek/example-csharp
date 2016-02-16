FROM ubuntu:14.04

ENV BUILD_TIMESTAMP 20160214

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

RUN echo "deb http://download.mono-project.com/repo/debian wheezy/snapshots/4.2.2.30 main" > /etc/apt/sources.list.d/mono-xamarin.list

RUN apt-get update -qq \
	&& apt-get install -y mono-mcs mono-devel ca-certificates-mono

ADD . .

RUN mcs src/LingotekDemo.cs -o:LingotekDemo.exe /reference:System.Net.Http.dll \
		/reference:System.Web.Extensions.dll /reference:System.Net.Http.Formatting.dll

RUN mv src/LingotekDemo.exe /usr/local/bin/LingotekDemo.exe

ENTRYPOINT mono /usr/local/bin/LingotekDemo.exe && cp -a /usr/local/bin/LingotekDemo.exe /output
