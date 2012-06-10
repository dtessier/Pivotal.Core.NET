/* Pivotal 5 Solutions Inc. - Core .NET library, communication layer.
 * 
 * Copyright (C) 2012  KASRA RASAEE
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 */
using System;
using System.Net.Sockets;

using System.Diagnostics;

using System.Text;

using Pivotal.Core.NET.Codec;
using Pivotal.Core.NET.Command;
using Pivotal.Core.NET.Utilities;

namespace Pivotal.Core.NET.Sockets {
    
	/// <summary>
	/// On connect delegate.
	/// </summary>
    public delegate void OnConnectDelegate(IClientSocket client);
	
	/// <summary>
	/// On receive delegate.
	/// </summary>
    public delegate void OnReceiveDelegate(IClientSocket client, ICommand command);
	
	/// <summary>
	/// On sent delegate.
	/// </summary>
    public delegate void OnSentDelegate(IClientSocket client, ICommand command);

	/// <summary>
	/// Client socket implementation of the IClientSocket interface, handles TCPv4 and TCPv6 communiation, with custom or default Codecs' e.g. Xml or Binary.
	/// </summary>
    public class ClientSocket : AbstractNetworking, IClientSocket {

        /// <summary>
        /// The maximum size of the receive buffer to allow, this could be partial, or one-or-more commands.
        /// 
        /// ** THE MAXIMUM SIZE OF THE BUFFER FOR EACH RECEIVED CALLBACK ON THIS SOCKET **
        /// 
        /// Example, one command can be 4096, meaning there will be a total of four asynchronous callbacks.
        /// Example, two commands can exist in 1024 bytes, or less, or more.
        /// 
        /// </summary>
        public Int32 MAX_RECEIVE_BUFFER_SIZE = 1024;

        /// <summary>
        /// Whether to invoke the <see cref="OnReceiveDelegate"/> event handler asynchronously? if <code>false</code> 
        /// socket will block until the event delegate is returned to the invocation point within 
        /// the callstack 
        /// 
        /// Default is <code>false</code>
        /// </summary>
        public Boolean OnReceivedInvokeAsynchronously { get; set; }

        /// <summary>
        /// The network socket 
        /// </summary>
        protected Socket Socket;

        /// <summary>
        /// Default read buffer area
        /// </summary>
        protected Byte[] BufferReceived = new Byte[1];

        /// <summary>
        /// Default Encoding for this socket stream (TODO implement encoding switch command)
        /// </summary>
        public Encoding BufferEncoding = Encoding.UTF8;

        /// <summary>
        /// Default Buffer Codec, used when parsing the data, usually an xml codec
        /// </summary>
		public IBufferCodec BufferCodec { get; set; }

        /// <summary>
        /// The incoming command stack, the command type could be identified before the entire stream has been put together
        /// 
        /// <see cref="Pivotal.Core.NET.Codec.IBufferCodec"/>
        /// </summary>
        protected System.Collections.Stack CommandStack = new System.Collections.Stack();

        /// <summary>
        /// Concrete Server OnConnectDelegate Method Handler
        /// 
        /// Invoke this method when the socket is ready; pushed to the server implementation handling this socket
        /// </summary>
        public event OnConnectDelegate OnConnected;

        /// <summary>
        /// Concrete Server OnReceived Method Handler
        /// 
		/// Invoke this method when a new command is ready; pushed to the server 
		/// implementation handling this socket or directly to the client application
		/// handling the OnReceive delegate.
		/// 
		/// Note: if Event Handlers are added for individual command types, then the 
		/// handler method will be invoked not the generic OnReceived delegate method.
        /// </summary>
        public event OnReceiveDelegate OnReceived;

        /// <summary>
        /// Concrete Server OnReceived Method Handler
        /// 
        /// Invoke this method when a command has been sent; pushed to the server implementation handling this socket
        /// </summary>
        public event OnSentDelegate OnSent;

        /// <summary>
        /// New TCP Socket IPv4
        /// </summary>
        /// <returns></returns>
        protected static Socket CreateSocketInstanceTCPv4() {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Default Constructor creating a new tcp sockets, and setting UTF8 encoding type.
        /// </summary>
        public ClientSocket() : this (ClientSocket.CreateSocketInstanceTCPv4()) {
            // TODO create IPv6 class hierarchy?
        }

        /// <summary>
        /// Connect to remote end point
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Connect(String host, Int32 port) {
			//this.Socket.Connect (host, port);
			DoBeginConnect (host, port);
        }
		
		public void Send(ICommand command) {
			DoBeginSend (command);
		}

        /// <summary>
        /// Default socket implementation, uses default UTF8 encoding
        /// </summary>
        /// <param name="socket"></param>
        public ClientSocket(Socket socket) : this(socket, Encoding.UTF8) {
            // call this(constructor)
        }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Pivotal.Core.NET.Sockets.ClientSocket"/> class.
		/// </summary>
		/// <param name='socket'>
		/// Socket.
		/// </param>
		/// <param name='bufferEncoding'>
		/// Buffer encoding.
		/// </param>
		public ClientSocket(Socket socket, Encoding bufferEncoding) : this (socket, bufferEncoding, null) {
			// call another constructor.
			// do nothing else here..
		}
		
        /// Constructor, takes in socket and default buffer encoding
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="bufferEncoding"></param>
        /// <param name="bufferCodec"></param>
		public ClientSocket (Socket socket, Encoding bufferEncoding, IBufferCodec bufferCodec) {
            Socket = socket;

            if (bufferEncoding != null) {
                BufferEncoding = bufferEncoding;
            }
			
			if (bufferCodec != null) {
				this.BufferCodec = bufferCodec;
			}

            if (socket == null) {
                // TODO exception?
            }

            SetDefaults();

            DoBeginReceive(null);
		}

        /// <summary>
        /// Begin Receive
        /// </summary>
        /// <param name="previousBuffer">previous buffer not yet cut</param>
        protected virtual void DoBeginReceive(Byte[] previousBuffer) {
            Socket.BeginReceive(BufferReceived, 0, BufferReceived.Length, SocketFlags.None, BeginReceiveCallback, previousBuffer);
        }
		
		/// <summary>
		/// Do the begin connect asynchronously
		/// </summary>
		/// <param name='host'>
		/// Host.
		/// </param>
		/// <param name='port'>
		/// Port.
		/// </param>
		protected virtual void DoBeginConnect(String host, Int32 port) {
			this.Socket.BeginConnect (host, port, BeginConnectCallback, this);
		}
		
		/// <summary>
		/// Serialize and send command asynchronously
		/// </summary>
		/// <param name='command'>
		/// Command.
		/// </param>
		protected virtual void DoBeginSend(ICommand command) {
			// Serializer...
			SocketError errorCode;
			Byte[] buffer = BufferCodec.ToByteArray (command, this.BufferEncoding);
			
			if (!Comparison.IsEmpty(buffer)) {
				// begin asynchronous send
				this.Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out errorCode, BeginSendCallback, command);
			}
		}

        /// <summary>
        /// Defaults when creating an instance of <see cref="ClientSocket"/>
        /// </summary>
        protected void SetDefaults() {
            this.OnReceivedInvokeAsynchronously = false;
			if (this.BufferCodec == null) {	
            	this.BufferCodec = XmlBufferCodec.SingletonInstance();
			}
        }
		
		/// <summary>
		/// Invokes (delegates) connected method within the associated server or client running program.
		/// </summary>
		internal void InvokeConnected () {
			if (this.OnConnected != null) {
				this.OnConnected (this);
			}
			return ;
		}

		/// <summary>
		/// Begins the connect callback.
		/// </summary>
		/// <param name='result'>
		/// Result.
		/// </param>
		protected void BeginConnectCallback(IAsyncResult result) {
			Socket.EndConnect (result);
			InvokeConnected ();
		}
		
        /// <summary>
        /// Callback method used when data has been sent the socket asynchronously
        /// </summary>
        /// <param name="result"></param>
        protected void BeginSendCallback(IAsyncResult result) {
            SocketError errorCode; // undefined
            int sent = Socket.EndSend(result, out errorCode);

            // TODO complete

            // TODO invoke OnSent delegate (if not null)
        }

        /// <summary>
        /// Callback when data received on a socket (usually when there is x amount of data available) 
        /// </summary>
        ///
        /// <note>
        ///     This does not necessarily mean the entire content of the data has arrived, could very well 
        ///     be partial data received. usually a \0 would terminate the command in case of a text stream.
        /// </note>
        /// <param name="result"></param>
        protected void BeginReceiveCallback(IAsyncResult result) {
            int read = Socket.EndReceive(result);
            if (read > 0) {

                // previous buffer (could be blank null end-of- process the bytes
                Byte[] previousBuffer = null; // used as a staging area until the command is ready to be pushed to the server implementation

                if (result.AsyncState is Byte[]) {
                    // as ? lol, I guess you can cast this way too with C#
                    previousBuffer = result.AsyncState as byte[];
                }

                // storage area for the available bytes on the buffer
                Byte[] readData = new Byte[read];

                // DEBUG (TODO REMOVE?)
               // Console.Write(BufferEncoding.GetString(readData));


                // copy the available bytes to the data buffer
                Buffer.BlockCopy(BufferReceived, 0, readData, 0, read);

                // process the bytes received
                ProcessReceiveBuffer(readData, ref previousBuffer, 1);

                // get the next receive buffer size.
               // Int32 receiveCapacity = IncreaseReceiveBufferCapacity(read);

                // startup the receiver - asynchronously
                DoBeginReceive(previousBuffer);

                //DoBeginReceive(
                //Socket.BeginReceive(BufferReceived, 0, receiveCapacity, SocketFlags.None, BeginReceiveCallback, previousBuffer);
            }
        }

        protected virtual Int32 IncreaseReceiveBufferCapacity(Int32 count) {
            if (count == BufferReceived.Length && count >= MAX_RECEIVE_BUFFER_SIZE) {
                count *= 2;
                BufferReceived = new byte[count];
            }

            return count;
        }

        /// <summary>
        /// Merge the two buffers offset 0 to maximum length on both buffers
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        protected byte[] MergeBuffer(byte[] first, byte[] second) {
            return MergeBuffer(first, 0, -1, second, 0, -1);
        }

        /// <summary>
        /// Merge the buffers start offset zero on both buffers, and to a given count on each buffer
        /// </summary>
        /// <param name="first"></param>
        /// <param name="firstCount"></param>
        /// <param name="second"></param>
        /// <param name="secondCount"></param>
        /// <returns></returns>
        protected byte[] MergeBuffer(byte[] first, int firstCount, byte[] second, int secondCount) {
            return MergeBuffer(first, 0, firstCount, second, 0, secondCount);
        }

        /// <summary>
        /// Merge the buffers start offset zero on both buffers, but to a maximum length on the second buffer
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="secondCount"></param>
        /// <returns></returns>
        protected byte[] MergeBuffer(byte[] first, byte[] second, int secondCount) {
            return MergeBuffer(first, -1, second, secondCount);
        }

        /// <summary>
        /// Merge the buffers start offset zero on both buffers, but to a maximum length on the first buffer
        /// </summary>
        /// <param name="first"></param>
        /// <param name="firstCount"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        protected byte[] MergeBuffer(byte[] first, int firstCount, byte[] second) {
            return MergeBuffer(first, 0, firstCount, second, 0, -1);
        }


        protected byte[] MergeBuffer(
            byte[] buf1, int pos1, int count1, 
            byte[] buf2, int pos2, int count2) {

            #region CALCULATE REGIONS
            int len1 = buf1 != null ? buf1.Length : 0;
            int len2 = buf2 != null ? buf2.Length : 0;

            count1 = count1 == -1 ? len1 : count1;
            count2 = count2 == -1 ? len2 : count2;
            #endregion;

            #region CHECK OUT OF RANGE

            if (count1 > len1) {
                throw new IndexOutOfRangeException(String.Format("Copy count of {0} > {1} within first buffer", count1, len1));
            }

            if (count2 > len2) {
                throw new IndexOutOfRangeException(String.Format("Copy count of {0} > {1} within second buffer", count2, len2));
            }

            if (pos1 > count1) {
                throw new IndexOutOfRangeException(String.Format("Start position of {0} > {1} the first buffer count", pos1, count1));
            }

            if (pos2 > count2) {
                throw new IndexOutOfRangeException(String.Format("Start position of {0} > {1} the second buffer count", pos2, count2));
            }

            #endregion

            #region MERGE BUFFERS
            // the total size of the buffer
            int t = count1 + count2 - pos1 - pos2;

            byte[] merge = new byte[t];
            if (count1 > 0) {
                Buffer.BlockCopy(buf1, pos1, merge, 0, count1);
            }

            if (count2 > 0) {
                Buffer.BlockCopy(buf2, pos2, merge, count1, count2);
            }
            #endregion

            return merge;
        }

		

        protected virtual ICommand ProcessReceiveBuffer(Byte[] data, ref Byte[] remainder, int depth) {

			/// TODO seriously clean this code up, break it up into smaller methods.
			
			/// TODO seriously clean this code up, break it up into smaller methods.
			
			/// TODO seriously clean this code up, break it up into smaller methods.
			
			/// TODO seriously clean this code up, break it up into smaller methods.
			
			
			
			Boolean hasPrevBuffer = (remainder != null && remainder.Length > 0);
			Int32 lengthPrevBuffer = hasPrevBuffer ? remainder.Length : 0;

			// TODO should support streams instead of creating byte arrays.
			Int32 cutpos = Array.IndexOf<Byte> (data, ((byte)'\0'));

			if (cutpos >= 0) {
				
				
				// append the previous buffer to the new data buffer, only append to cutpos of data buffer
				// essentially, it will prefix data buf with remainder, and append the data buffer up to cutpos
				byte[] b = MergeBuffer (remainder, data, cutpos);

				Object serial = null;
				if (CommandStack.Count == depth) {
					serial = CommandStack.Pop ();
				} else {
					serial = BufferCodec.DetermineSerial (b, this.BufferEncoding);
				}
				
				// TODO determine if the command exists????
				
				ICommand command = BufferCodec.ToCommand (serial, b, this.BufferEncoding);

				if (command == null) {
					// TODO should dump this into a file? depending on the size?
					Debug.WriteLine (String.Format ("Unable to parse command using {0}: ", BufferEncoding.GetString (b)));
				} else {
					
					// attempt to invoke the appropriate handler, if it exists, otherwise
					// call the generic OnReceive handler. Usually event handlers exist when
					// the usage is a client and not a server endpoint, the AbstractServer 
					// will most likely handle all the event handling rather the actual client socket.
					if (!AttemptReceiveEventHandler (this, command, this.OnReceivedInvokeAsynchronously)) {
						
						if (OnReceivedInvokeAsynchronously) {
							this.OnReceived.BeginInvoke (this, command, BeginInvokeOnReceivedCallback, null);
						} else {
							this.OnReceived(this, command);
						}
					}
                }
				
				/// copy the data buffer remainder, so from cutpos onward to the remainder reference.
				/// however, first, recursively walk the array, searching for command splits \0
                if (data.Length > cutpos) {
                    int length = data.Length - cutpos - 1;
                    remainder = new byte[length];
                    Buffer.BlockCopy(data, cutpos + 1, remainder, 0, length);
                    return ProcessReceiveBuffer(remainder, ref remainder, depth+1);
                } else {
                    remainder = null;
                }

                // TODO return command (use appropriate deserializer)
                return default(ICommand);
            }

            // only if the data is not the same reference as the remainder.
            // this usually happens when the data buffer is split and there
            // are remaining bytes in the data stream that need to be returned
            // and available for the next socket receive buffer (prepending)
            if (data != remainder) {
                remainder = MergeBuffer(remainder, data);
            }

            // if there is enough data to determine the command type.
            
            if (depth > CommandStack.Count) {
                Object commandId = BufferCodec.DetermineSerial(remainder, this.BufferEncoding);
                if (commandId != null) {
                    CommandStack.Push(commandId);
                }
            }

            return default(ICommand);
        }
		
		/// <summary>
		/// Terminates the asynchronous invocation of the OnReceived(..) method, which is usually passed to the client or server running process.
		/// </summary>
		/// <param name='result'>
		/// Result.
		/// </param>
		protected virtual void BeginInvokeOnReceivedCallback(IAsyncResult result) {
			OnReceived.EndInvoke(result);
		}
		
		
		public override string ToString() {
			return string.Format ("[ClientSocket: Local={0}, Remote={1}]", Socket.LocalEndPoint, Socket.RemoteEndPoint);
		}
	}
}
