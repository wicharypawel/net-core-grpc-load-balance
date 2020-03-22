FROM gradle:jdk8 as builder
WORKDIR /app
COPY . .
COPY ./logging.properties ./build/install/examples/bin/logging.properties
RUN ./gradlew installDist
#ENV JAVA_TOOL_OPTIONS=-Djava.util.logging.config.file=logging.properties
CMD ./build/install/examples/bin/hello-world-client
