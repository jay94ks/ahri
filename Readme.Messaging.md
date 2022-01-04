## Ahri.Messaging
_The Simplest Message Delivery Framework_

_Use DI on Everywhere!_

Ahri.Messaging is the messaging framework that delivers messages between messaging connections.

### The protocol.
Basically, The message protocol is designed that based on binary data not text. 
and, encryption module can be inserted on packet receiver, sender, however,
it will not communicate about encryption informations.
so, nobody knows the protocol uses what encryption.

### Packet Framing.
```
0                          2                                              n (bytes)
+--------------------------+----------------------------------------------+
| Payload Length           | Payload Bytes                                |
+--------------------------+----------------------------------------------+
```

### Packet Format.
```
0           1                                                             n (bytes)
+-----------+-------------------------------------------------------------+
| Opcode    | Packet Contents                                             |
+-----------+-------------------------------------------------------------+
````

### Message Packet.
```
0           1              2                                              n (bytes)
+-----------+----------------------------------------------+--------------+
| 0x01      | GUID Bytes (16 bytes)                        | Type Length  |
+-----------+----------------------------------------------+--------------+
| Name (MAX: 255 bytes)                                                   |
+-------------------------------------------------------------------------+
| Remainders are request body                                             |
+-------------------------------------------------------------------------+
```

### Message Reply Packet.
```
0           1                              16             17              n (bytes)
+-----------+-------------------------------+--------------+--------------+
| 0x02      | GUID Bytes (16 bytes)         | Status Code  | Type Length  |
+-----------+-------------------------------+--------------+--------------+
| Name (MAX: 255 bytes)                                                   |
+-------------------------------------------------------------------------+
| Remainders are response body                                            |
+-------------------------------------------------------------------------+
```

Status Codes are:
```
-- Success codes (range: 0x00 ~ 0x7f)
Status: 0x00 Okay.						Request processing successful.
Status: 0x01 No Content.				Request processing successful, but no response body.

-- Error codes (range: 0x80 ~ 0xff)
Status: 0x80 Timeout.					Execution time limit reached.
Status: 0x81 No Request Implemented.	The target was unable to process the request.
Status: 0x82 No Request.				No such request. (not valid)
Status: 0x83 Request Decoding Failure.	Request received, but decoding failed.
Status: 0x84 Response Decoding Failure.	Response received, but decoding failed.
Status: 0x85 Suppressed.				The request was processed, but sending is prohibited.
Status: 0x86 No Response Implemented.	Response received, but unable to process the corresponding response body.
Status: 0x87 Request Encoding Failure.
Status: 0x88 Response Encoding Failure.
Status: 0x89 Request Too Long.			Request data too large to send.
Status: 0x90 Response Too Long.			Response data too large to send.
Status: 0x91 Forbidden.					Declined to process the request.

Status: 0xFD Max Concurrency Reached.	Request maximum concurrent throughput exceeded.
Status: 0xFE Request Aborted.			An attempt was made to send a request, but the connection was aborted.
Status: 0xFF Execution Failure.			The request was received, but an unhandled exception was thrown.
```

### Middlewares like Web.
```
Task MiddlewareDelegate(MessagingContext Context, Func<Task> Next);
```
