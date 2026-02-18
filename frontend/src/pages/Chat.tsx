import React, { useState, useEffect, useRef } from "react";
import { useAuth } from "../contexts/AuthContext";
import { chatService, fileService } from "../services/api";
import { ChatMessage, ChatSession } from "../types";
import "./Chat.css";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import rehypeHighlight from "rehype-highlight";
import "highlight.js/styles/github-dark.css";

const Chat: React.FC = () => {
  const [message, setMessage] = useState("");
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [sessions, setSessions] = useState<ChatSession[]>([]);
  const [currentSessionId, setCurrentSessionId] = useState<string>("");
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [attachments, setAttachments] = useState<string[]>([]);

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { user, logout } = useAuth();

  useEffect(() => {
    loadSessions();
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const loadSessions = async () => {
    try {
      const sessionsData = await chatService.getSessions();
      setSessions(sessionsData);
    } catch (err) {
      console.error("Error loading sessions:", err);
    }
  };

  const loadSession = async (sessionId: string) => {
    try {
      const messagesData = await chatService.getSessionMessages(sessionId);
      setMessages(messagesData);
      setCurrentSessionId(sessionId);
    } catch (err) {
      console.error("Error loading session messages:", err);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!message.trim() && attachments.length === 0) return;

    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      sessionId: currentSessionId,
      userId: user?.userId || "",
      role: "user",
      content: message,
      timestamp: new Date().toISOString(),
      attachments: [...attachments],
    };

    setMessages((prev) => [...prev, userMessage]);
    setMessage("");
    setAttachments([]);
    setLoading(true);

    try {
      const response = await chatService.sendMessage({
        message,
        sessionId: currentSessionId || undefined,
        attachments: userMessage.attachments,
      });

      const assistantMessage: ChatMessage = {
        id: Date.now().toString(),
        sessionId: response.sessionId,
        userId: user?.userId || "",
        role: "assistant",
        content: response.message,
        timestamp: response.timestamp,
        attachments: response.attachments || [],
      };

      setMessages((prev) => [...prev, assistantMessage]);
      setCurrentSessionId(response.sessionId);
      await loadSessions();
    } catch (err: any) {
      console.error("Error sending message:", err);
      setMessages((prev) => [
        ...prev,
        {
          id: Date.now().toString(),
          sessionId: currentSessionId,
          userId: user?.userId || "",
          role: "assistant",
          content: "Sorry, there was an error processing your message.",
          timestamp: new Date().toISOString(),
          attachments: [],
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    try {
      const url = await fileService.uploadFile(file);
      setAttachments((prev) => [...prev, url]);
    } catch (err) {
      console.error("Error uploading file:", err);
    } finally {
      setUploading(false);
    }
  };

  const startNewChat = () => {
    setMessages([]);
    setCurrentSessionId("");
    setAttachments([]);
  };

  return (
    <div className="chat-container">
      <div className="chat-sidebar">
        <div className="sidebar-header">
          <h2>ColumbiaAI</h2>
          <button onClick={startNewChat} className="new-chat-btn">
            + New Chat
          </button>
        </div>
        <div className="sessions-list">
          <h3>Chat History</h3>
          {sessions.map((session) => (
            <div
              key={session.id}
              className={`session-item ${
                session.id === currentSessionId ? "active" : ""
              }`}
              onClick={() => loadSession(session.id)}
            >
              <div className="session-title">{session.title}</div>
              {/* <div className="session-date">
                {new Date(session.updatedAt).toLocaleDateString()}
              </div> */}
            </div>
          ))}
        </div>
        <div className="sidebar-footer">
          <div className="user-info">
            <span>{user?.name}</span>
          </div>
          <button onClick={logout} className="logout-btn">
            Logout
          </button>
        </div>
      </div>

      <div className="chat-main">
        <div className="chat-messages">
          {messages.length === 0 ? (
            <div className="empty-state">
              <h2>Welcome to ColumbiaAI</h2>
              <p>Start a conversation by typing a message below</p>
            </div>
          ) : (
            messages.map((msg, index) => (
              <div key={index} className={`message ${msg.role}`}>
                <div className="message-content">
                  <ReactMarkdown
                    remarkPlugins={[remarkGfm]}
                    rehypePlugins={[rehypeHighlight]}
                    components={{
                      code({ className, children, ...props }) {
                        const match = /language-(\w+)/.exec(className || "");
                        return match ? (
                          <pre className="chat-code-block">
                            <code className={className} {...props}>
                              {children}
                            </code>
                          </pre>
                        ) : (
                          <code className="inline-code" {...props}>
                            {children}
                          </code>
                        );
                      },
                    }}
                  >
                    {msg.content}
                  </ReactMarkdown>

                  {(msg.attachments || []).length > 0 && (
                    <div className="attachments">
                      {(msg.attachments || []).map((url, i) => (
                        <div key={i} className="attachment-item">
                          <a
                            href={url}
                            target="_blank"
                            rel="noopener noreferrer"
                          >
                            {url.split("/").pop()}
                          </a>
                          {url.match(/\.(jpg|jpeg|png|gif)$/i) && (
                            <img
                              src={url}
                              alt="attachment"
                              className="file-image-preview"
                            />
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            ))
          )}
          {loading && (
            <div className="message assistant">
              <div className="message-content">Thinking...</div>
            </div>
          )}
          <div ref={messagesEndRef} />
        </div>

        <form className="chat-input-form" onSubmit={handleSendMessage}>
          <div className="message-input-wrapper">
            {/* Left Icon */}
            <span
              className="input-left-icon"
              onClick={() => fileInputRef.current?.click()}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={2}
                stroke="currentColor"
                width={20}
                height={20}
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M12 5v14m7-7H5"
                />
              </svg>
            </span>

            {/* Input Area */}
            <div className="input-area">
              {/* Attachment Previews */}
              {attachments.length > 0 && (
                <div className="attachments-preview-inline">
                  {attachments.map((url, i) => (
                    <div key={i} className="attachment-preview-item-inline">
                      {url.match(/\.(jpg|jpeg|png|gif)$/i) ? (
                        <img
                          src={url}
                          alt="attachment"
                          className="attachment-preview-image-inline"
                        />
                      ) : (
                        <span className="attachment-preview-file-inline">
                          {url.split("/").pop()}
                        </span>
                      )}
                      <button
                        type="button"
                        className="remove-attachment-btn-inline"
                        onClick={() =>
                          setAttachments((prev) =>
                            prev.filter((_, index) => index !== i)
                          )
                        }
                      >
                        ✕
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {/* Textarea */}
              <textarea
                className="message-input"
                value={message}
                placeholder="Type your message..."
                onChange={(e) => {
                  setMessage(e.target.value);
                  e.target.style.height = "auto";
                  e.target.style.height = e.target.scrollHeight + "px";
                }}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault();
                    handleSendMessage(e as any);
                  }
                }}
              />
            </div>

            {/* Send Icon */}
            <span className="input-right-icon">
              <button
                type="submit"
                className="send-btn"
                disabled={!message.trim() && attachments.length === 0}
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  strokeWidth={2}
                  stroke="currentColor"
                  width={16}
                  height={16}
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M12 19l7-7-7-7M5 12h14"
                  />
                </svg>
              </button>
            </span>
          </div>

          <input
            type="file"
            ref={fileInputRef}
            style={{ display: "none" }}
            onChange={handleFileUpload}
          />

          <input
            type="file"
            ref={fileInputRef}
            style={{ display: "none" }}
            onChange={handleFileUpload}
          />
        </form>
      </div>
    </div>
  );
};

export default Chat;
