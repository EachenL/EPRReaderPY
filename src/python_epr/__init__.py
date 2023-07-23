import grpc
import binding.grpc.reader_pb2 as reader
import binding.grpc.reader_pb2_grpc as reader_grpc
import binding.grpc.writer_pb2 as writer
import binding.grpc.writer_pb2_grpc as writer_grpc


def read(file_path: str, read_header_only=False):
    channel = grpc.insecure_channel('127.0.0.1:6082')
    stub = reader_grpc.ReaderStub(channel)
    res = stub.ReadEpr(reader.ReadRequest(
        file_path=file_path,
        read_header_only=read_header_only))
    return res.epr


def write(file_path: str, epr):
    channel = grpc.insecure_channel('127.0.0.1:6082')
    stub = writer_grpc.WriterStub(channel)
    res = stub.WriteEpr(writer.WriteRequest(
        file_path=file_path,
        epr=epr))
    return res.message
