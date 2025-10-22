export interface User {
  id: string;
  email: string;
  name: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
}

export interface ChatMessage {
  id: string;
  sessionId: string;
  userId: string;
  role: "user" | "assistant";
  content: string;
  timestamp: string;
  attachments?: string[]; // optional file URL
}

export interface ChatSession {
  id: string;
  userId: string;
  title: string;
  createdAt: string;
  updatedAt: string;
  context?: string;
  isActive: boolean;
}

export interface ChatRequest {
  message: string;
  sessionId?: string;

  attachments?: string[];
}

export interface ChatResponse {
  message: string;
  sessionId: string;
  timestamp: string;
  attachments?: string[];
}

export interface UserProfile {
  preferences: { [key: string]: string };
  interests: string[];
  context: string;
}
