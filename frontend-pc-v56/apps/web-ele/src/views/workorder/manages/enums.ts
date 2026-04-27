
export enum WorkOrderStatus {
  Draft = 0, // 草稿
  PendingAssignment = 1, // 待分派
  InProgress = 2, // 处理中
  PendingApproval = 3, // 待审核
  Completed = 4, // 已完成
  Canceled = 5, // 已作废
}

export enum WorkOrderPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum WorkOrderTrigger {
  Submit = 0, // 提交
  Assign = 1, // 分派
  Start = 2, // 开始执行
  Finish = 3, // 完成上报
  Approve = 4, // 审核通过
  Reject = 5, // 驳回
  Cancel = 6, // 作废
}

export const actionEnum = {
  none: 'none',
  add: 'add',
  edit: 'edit',
  delete: 'delete',
};
