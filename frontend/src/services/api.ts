import axios from 'axios';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  ChatRequest,
  ChatResponse,
  ChatSession,
  ChatMessage,
  UserProfile,
} from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authService = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/register', data);
    return response.data;
  },
};

export const chatService = {
  sendMessage: async (data: ChatRequest): Promise<ChatResponse> => {
    const response = await api.post<ChatResponse>('/chat/message', data);
    return response.data;
  },

  getSessions: async (): Promise<ChatSession[]> => {
    const response = await api.get<ChatSession[]>('/chat/sessions');
    return response.data;
  },

  getSessionMessages: async (sessionId: string): Promise<ChatMessage[]> => {
    const response = await api.get<ChatMessage[]>(`/chat/sessions/${sessionId}/messages`);
    return response.data;
  },

  continueSession: async (sessionId: string): Promise<ChatSession> => {
    const response = await api.post<ChatSession>(`/chat/sessions/${sessionId}/continue`);
    return response.data;
  },

  searchDocuments: async (query: string): Promise<string[]> => {
    const response = await api.post<string[]>('/chat/search', JSON.stringify(query), {
      headers: { 'Content-Type': 'application/json' },
    });
    return response.data;
  },
};

export const fileService = {
  uploadFile: async (file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<{ url: string }>('/file/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data.url;
  },
};

export const userService = {
  getProfile: async (): Promise<any> => {
    const response = await api.get('/user/profile');
    return response.data;
  },

  updateProfile: async (profile: UserProfile): Promise<any> => {
    const response = await api.put('/user/profile', profile);
    return response.data;
  },
};

export default api;
