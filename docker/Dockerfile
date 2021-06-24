FROM mono

RUN apt-get update -y && apt-get install -y nunit-console git osslsigncode

RUN ln -s /src/docker/build.sh /
CMD /build.sh

ADD packages.config /packages/
RUN nuget restore /packages/packages.config -PackagesDirectory /packages
