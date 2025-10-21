import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { chatService, fileService } from '../services/api';
import { ChatMessage, ChatSession } from '../types';
import './Chat.css';

const Chat: React.FC = () => {
  const [message, setMessage] = useState('');
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [sessions, setSessions] = useState<ChatSession[]>([]);
  const [currentSessionId, setCurrentSessionId] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { user, logout } = useAuth();

  useEffect(() => {
    loadSessions();
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const loadSessions = async () => {
    try {
      const sessionsData = await chatService.getSessions();
      setSessions(sessionsData);
    } catch (err) {
      console.error('Error loading sessions:', err);
    }
  };

  const loadSession = async (sessionId: string) => {
    try {
      const messagesData = await chatService.getSessionMessages(sessionId);
      setMessages(messagesData);
      setCurrentSessionId(sessionId);
    } catch (err) {
      console.error('Error loading session messages:', err);
    }
  };

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!message.trim()) return;

    const userMessage: ChatMessage = {
      id: Date.now().toString(),
      sessionId: currentSessionId,
      userId: user?.userId || '',
      role: 'user',
      content: message,
      timestamp: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setMessage('');
    setLoading(true);

    try {
      const response = await chatService.sendMessage({
        message: message,
        sessionId: currentSessionId || undefined,
      });

      const assistantMessage: ChatMessage = {
        id: Date.now().toString(),
        sessionId: response.sessionId,
        userId: user?.userId || '',
        role: 'assistant',
        content: response.message,
        timestamp: response.timestamp,
      };

      setMessages((prev) => [...prev, assistantMessage]);
      setCurrentSessionId(response.sessionId);
      await loadSessions();
    } catch (err: any) {
      console.error('Error sending message:', err);
      const errorMessage: ChatMessage = {
        id: Date.now().toString(),
        sessionId: currentSessionId,
        userId: user?.userId || '',
        role: 'assistant',
        content: 'Sorry, there was an error processing your message. Please try again.',
        timestamp: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, errorMessage]);
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
      console.log('File uploaded:', url);
    } catch (err) {
      console.error('Error uploading file:', err);
    } finally {
      setUploading(false);
    }
  };

  const startNewChat = () => {
    setMessages([]);
    setCurrentSessionId('');
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
              className={`session-item ${session.id === currentSessionId ? 'active' : ''}`}
              onClick={() => loadSession(session.id)}
            >
              <div className="session-title">{session.title}</div>
              <div className="session-date">
                {new Date(session.updatedAt).toLocaleDateString()}
              </div>
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
                <div className="message-content">{msg.content}</div>
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
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileUpload}
            style={{ display: 'none' }}
          />
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            disabled={uploading}
            className="attach-btn"
          >
            📎
          </button>
          <input
            type="text"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            placeholder="Type your message..."
            disabled={loading}
            className="message-input"
          />
          <button type="submit" disabled={loading || !message.trim()} className="send-btn">
            Send
          </button>
        </form>
      </div>
    </div>
  );
};

export default Chat;
