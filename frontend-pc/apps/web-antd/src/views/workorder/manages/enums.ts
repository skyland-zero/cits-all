export enum WorkOrderStatus {
  Draft = 0,
  PendingAssignment = 1,
  InProgress = 2,
  PendingApproval = 3,
  Completed = 4,
  Canceled = 5,
}

export enum WorkOrderPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum actionEnum {
  none = 0,
  add = 1,
  edit = 2,
  delete = 3,
  view = 4,
}
