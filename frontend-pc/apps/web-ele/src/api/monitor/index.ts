import { requestClient } from '#/api/request';

export interface SystemInfo {
  os: string;
  architecture: string;
  framework: string;
  startTime: string;
  upTime: string;
}

export interface CpuInfo {
  name: string;
  cores: number;
  usage: number;
}

export interface MemoryInfo {
  total: number;
  used: number;
  frameworkUsed: number;
}

export interface DiskInfo {
  name: string;
  total: number;
  free: number;
}

export interface ServiceStatus {
  name: string;
  status: 'Online' | 'Offline';
  latency: number;
  details?: Record<string, string>;
}

export interface ServerInfoResult {
  system: SystemInfo;
  cpu: CpuInfo;
  memory: MemoryInfo;
  disk: DiskInfo[];
  services: ServiceStatus[];
  onlineUsers: number;
}

/**
 * 获取服务器监控信息
 */
export async function getServerInfoApi() {
  return requestClient.get<ServerInfoResult>('/monitor/server-info');
}
