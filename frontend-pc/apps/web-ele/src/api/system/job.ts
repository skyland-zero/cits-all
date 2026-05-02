import { requestClient } from '#/api/request';

const Api = {
  Stats: '/system/jobs/stats',
  Processing: '/system/jobs/processing',
  Failed: '/system/jobs/failed',
  Recurring: '/system/jobs/recurring',
};

export function getJobStats() {
  return requestClient.get(Api.Stats);
}

export function getProcessingJobs(from = 0, count = 20) {
  return requestClient.get(Api.Processing, { params: { from, count } });
}

export function getFailedJobs(from = 0, count = 20) {
  return requestClient.get(Api.Failed, { params: { from, count } });
}

export function getRecurringJobs() {
  return requestClient.get(Api.Recurring);
}

export function triggerRecurringJob(id: string) {
  return requestClient.post(`${Api.Recurring}/${id}/trigger`);
}

export function removeRecurringJob(id: string) {
  return requestClient.delete(`${Api.Recurring}/${id}`);
}

export function deleteJob(id: string) {
  return requestClient.delete(`/system/jobs/${id}`);
}
