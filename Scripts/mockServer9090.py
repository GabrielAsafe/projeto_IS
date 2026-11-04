
from http.server import BaseHTTPRequestHandler, HTTPServer
import json

class Handler(BaseHTTPRequestHandler):
    def do_POST(self):

        if "chunked" in self.headers.get("Transfer-Encoding", "").lower():
            chunks = []

            while True:
                line = self.rfile.readline()
                size = int(line.strip().split(b";")[0], 16)

                if size == 0:
                    # Consumir os trailers HTTP
                    while self.rfile.readline() not in (b"\r\n", b"\n", b""):
                        pass
                    break

                chunks.append(self.rfile.read(size))
                self.rfile.read(2)  # CRLF

            body = b"".join(chunks).decode("utf-8")

        else:
            length = int(self.headers.get("Content-Length", 0))
            body = self.rfile.read(length).decode("utf-8")

        print("\nNotification received!")
        print("Path:", self.path)
        print("Body:", body)

        try:
            data = json.loads(body)
            print("Event:", data.get("event"))
            print("Resource:", data.get("resource"))
            print("Path:", data.get("path"))
        except json.JSONDecodeError:
            print("Invalid JSON!")
            self.send_response(400)
            self.end_headers()
            return

        self.send_response(200)
        self.end_headers()
        self.wfile.write(b"OK")


print("Listening on http://localhost:9090")
HTTPServer(("127.0.0.1", 9090), Handler).serve_forever()
