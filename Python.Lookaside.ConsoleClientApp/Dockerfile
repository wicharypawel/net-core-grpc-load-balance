FROM python:3
RUN pip install grpcio
RUN pip install grpcio-tools
COPY . .
CMD [ "python", "./greeter_client.py" ]