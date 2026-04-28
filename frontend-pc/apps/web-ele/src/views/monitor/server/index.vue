<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { useIntervalFn } from '@vueuse/core';
import {
  ElButton,
  ElCard,
  ElCol,
  ElDescriptions,
  ElDescriptionsItem,
  ElProgress,
  ElRow,
  ElTag,
} from 'element-plus';

import { EchartsUI, useEcharts, type EchartsUIType } from '@vben/plugins/echarts';

import { getServerInfoApi, type ServerInfoResult } from '#/api/monitor';

const serverInfo = ref<ServerInfoResult | null>(null);
const cpuUsageHistory = ref<number[]>(new Array(30).fill(0));
const memoryUsageHistory = ref<number[]>(new Array(30).fill(0));
const timeLabels = ref<string[]>(new Array(30).fill(''));

const cpuChartRef = ref<EchartsUIType>();
const { updateDate: updateCpuChart } = useEcharts(cpuChartRef);

const memChartRef = ref<EchartsUIType>();
const { updateDate: updateMemChart } = useEcharts(memChartRef);

const fetchData = async () => {
  try {
    const data = await getServerInfoApi();
    serverInfo.value = data;

    // 更新图表数据
    const now = new Date().toLocaleTimeString();

    // CPU 使用率
    const cpuUsage = data.cpu?.usage ?? (data as any).Cpu?.Usage ?? 0;
    cpuUsageHistory.value.push(cpuUsage);
    cpuUsageHistory.value.shift();

    // 内存 进程占用 (MB)
    const memUsed = data.memory?.used ?? (data as any).Memory?.Used ?? 0;
    memoryUsageHistory.value.push(memUsed);
    memoryUsageHistory.value.shift();

    timeLabels.value.push(now);
    timeLabels.value.shift();

    updateCharts();
  } catch (error) {
    console.error('获取监控数据失败:', error);
  }
};

const updateCharts = () => {
  updateCpuChart({
    grid: { bottom: '3%', containLabel: true, left: '3%', right: '4%' },
    series: [
      {
        areaStyle: { opacity: 0.3 },
        data: cpuUsageHistory.value,
        itemStyle: { color: '#409efb' },
        name: 'CPU使用率',
        smooth: true,
        type: 'line',
      },
    ],
    title: { text: 'CPU 进程占用 (%)' },
    tooltip: { trigger: 'axis' },
    xAxis: { boundaryGap: false, data: timeLabels.value, type: 'category' },
    yAxis: { max: 100, min: 0, type: 'value' },
  });

  updateMemChart({
    grid: { bottom: '3%', containLabel: true, left: '3%', right: '4%' },
    series: [
      {
        areaStyle: { opacity: 0.3 },
        data: memoryUsageHistory.value,
        itemStyle: { color: '#67c23a' },
        name: '内存占用',
        smooth: true,
        type: 'line',
      },
    ],
    title: { text: '内存 进程占用 (MB)' },
    tooltip: { trigger: 'axis' },
    xAxis: { boundaryGap: false, data: timeLabels.value, type: 'category' },
    yAxis: { min: 0, type: 'value' },
  });
};

const { isActive, pause, resume } = useIntervalFn(fetchData, 5000);

onMounted(() => {
  fetchData();
});
</script>

<template>
  <div class="p-5">
    <div
      class="mb-5 flex items-center justify-between rounded border bg-white p-3 shadow-sm"
    >
      <div class="flex items-center gap-2">
        <el-tag :type="isActive ? 'success' : 'info'" size="small">
          {{ isActive ? '正在监控' : '已暂停' }}
        </el-tag>
        <el-tag v-if="serverInfo && serverInfo.onlineUsers >= 0" type="primary" size="small" effect="plain">
          在线人数: {{ serverInfo.onlineUsers }}
        </el-tag>
        <span class="text-sm text-gray-500">数据每 5 秒自动更新一次</span>
      </div>
      <el-button
        :type="isActive ? 'warning' : 'success'"
        size="small"
        @click="isActive ? pause() : resume()"
      >
        {{ isActive ? '暂停自动刷新' : '恢复自动刷新' }}
      </el-button>
    </div>

    <el-row :gutter="20">
      <el-col :md="12" :sm="24">
        <el-card header="服务器信息" shadow="never">
          <el-descriptions :column="1" border v-if="serverInfo">
            <el-descriptions-item label="操作系统">
              {{ serverInfo.system.os }}
            </el-descriptions-item>
            <el-descriptions-item label="系统架构">
              {{ serverInfo.system.architecture }}
            </el-descriptions-item>
            <el-descriptions-item label="运行环境">
              {{ serverInfo.system.framework }}
            </el-descriptions-item>
            <el-descriptions-item label="启动时间">
              {{ new Date(serverInfo.system.startTime).toLocaleString() }}
            </el-descriptions-item>
            <el-descriptions-item label="运行时间">
              {{ serverInfo.system.upTime }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
      <el-col :md="12" :sm="24">
        <el-card header="服务状态" shadow="never">
          <div v-if="serverInfo" class="flex flex-col gap-4">
            <div
              v-for="service in serverInfo.services"
              :key="service.name"
              class="last:border-0 flex flex-col justify-between border-b p-3"
            >
              <div class="flex items-center justify-between mb-2">
                <span class="font-bold text-gray-700">{{ service.name }}</span>
                <div class="flex items-center gap-4">
                  <el-tag
                    :type="service.status === 'Online' ? 'success' : 'danger'"
                    effect="dark"
                  >
                    {{ service.status }}
                  </el-tag>
                  <span class="font-mono text-sm text-gray-500">
                    {{ service.latency }} ms
                  </span>
                </div>
              </div>
              <div v-if="service.details && Object.keys(service.details).length > 0" class="flex flex-wrap gap-2">
                <el-tag v-for="(val, key) in service.details" :key="key" type="info" effect="plain" size="small">
                  {{ key }}: {{ val }}
                </el-tag>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" class="mt-5">
      <el-col :md="12" :sm="24">
        <el-card shadow="never">
          <EchartsUI ref="cpuChartRef" style="height: 300px" />
        </el-card>
      </el-col>
      <el-col :md="12" :sm="24">
        <el-card shadow="never">
          <EchartsUI ref="memChartRef" style="height: 300px" />
        </el-card>
      </el-col>
    </el-row>

    <el-card class="mt-5" header="磁盘状态" shadow="never">
      <el-row :gutter="20" v-if="serverInfo">
        <el-col
          v-for="disk in serverInfo.disk"
          :key="disk.name"
          :lg="6"
          :md="12"
          :sm="24"
          class="mb-4"
        >
          <div
            class="rounded-xl border border-gray-100 bg-white p-5 shadow-sm transition-all hover:shadow-md"
          >
            <div class="mb-4 flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div
                  class="flex h-10 w-10 items-center justify-center rounded-full bg-blue-50 text-blue-500"
                >
                  <svg
                    class="lucide lucide-hard-drive"
                    fill="none"
                    height="20"
                    stroke="currentColor"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    viewBox="0 0 24 24"
                    width="20"
                    xmlns="http://www.w3.org/2000/svg"
                  >
                    <line x1="22" x2="2" y1="12" y2="12" />
                    <path
                      d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z"
                    />
                    <line x1="6" x2="6.01" y1="16" y2="16" />
                    <line x1="10" x2="10.01" y1="16" y2="16" />
                  </svg>
                </div>
                <span class="text-lg font-bold text-gray-700">{{ disk.name }}</span>
              </div>
              <el-tag effect="plain" type="info">{{ disk.total.toFixed(1) }} GB</el-tag>
            </div>

            <div class="mb-2 mt-4 flex items-end justify-between">
              <div class="text-sm text-gray-500">
                已用
                <span class="text-lg font-bold text-gray-800">
                  {{ (disk.total - disk.free).toFixed(1) }}
                </span>
                GB
              </div>
              <div class="text-sm text-gray-400">
                剩余 {{ disk.free.toFixed(1) }} GB
              </div>
            </div>

            <el-progress
              :color="[
                { color: '#67c23a', percentage: 60 },
                { color: '#e6a23c', percentage: 80 },
                { color: '#f56c6c', percentage: 100 },
              ]"
              :percentage="
                disk.total > 0
                  ? Math.round(((disk.total - disk.free) / disk.total) * 100)
                  : 0
              "
              :show-text="false"
              :stroke-width="12"
            />
          </div>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<style scoped></style>
