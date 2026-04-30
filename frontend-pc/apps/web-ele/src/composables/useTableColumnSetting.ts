import { computed, ref, watch } from 'vue';

import { useUserStore } from '@vben/stores';

export type TableColumnFixed = boolean | 'left' | 'right';

export interface TableColumnSettingColumn {
  key: string;
  label: string;
  prop?: string;
  visible?: boolean;
  width?: number | string;
  minWidth?: number | string;
  align?: 'center' | 'left' | 'right';
  fixed?: TableColumnFixed;
  locked?: boolean;
  hideable?: boolean;
  draggable?: boolean;
  setting?: boolean;
}

interface TableColumnSettingOptions {
  columns: TableColumnSettingColumn[];
  pageKey: string;
  storageKey?: string;
  minVisibleCount?: number;
}

interface TableColumnSettingStoragePayload {
  columns: Array<{
    key: string;
    order: number;
    visible: boolean;
  }>;
  updatedAt: number;
  version: 1;
}

const STORAGE_PREFIX = 'web-ele-table-column-setting';

function getFullCacheKey(key: string) {
  return `${STORAGE_PREFIX}-${key}`;
}

function getCacheItem<T>(key: string): null | T {
  if (typeof window === 'undefined') {
    return null;
  }

  const item = window.localStorage.getItem(getFullCacheKey(key));
  if (!item) {
    return null;
  }

  try {
    return JSON.parse(item) as T;
  } catch {
    window.localStorage.removeItem(getFullCacheKey(key));
    return null;
  }
}

function setCacheItem<T>(key: string, value: T) {
  if (typeof window === 'undefined') {
    return;
  }

  window.localStorage.setItem(getFullCacheKey(key), JSON.stringify(value));
}

function normalizeColumns(columns: TableColumnSettingColumn[]) {
  return columns.map((column) => {
    const normalized = {
      ...column,
      draggable: column.draggable ?? true,
      hideable: column.hideable ?? true,
      setting: column.setting ?? true,
      visible: column.visible ?? true,
    };

    if (normalized.locked) {
      normalized.draggable = false;
      normalized.hideable = false;
      normalized.visible = true;
    }

    if (!normalized.hideable) {
      normalized.visible = true;
    }

    return normalized;
  });
}

function mergeColumns(
  defaultColumns: TableColumnSettingColumn[],
  payload: null | TableColumnSettingStoragePayload,
) {
  if (!payload || payload.version !== 1) {
    return defaultColumns;
  }

  const defaultColumnMap = new Map(
    defaultColumns.map((column) => [column.key, column]),
  );
  const savedColumns = [...payload.columns].sort((a, b) => a.order - b.order);
  const mergedColumns: TableColumnSettingColumn[] = [];
  const usedKeys = new Set<string>();

  savedColumns.forEach((savedColumn) => {
    const defaultColumn = defaultColumnMap.get(savedColumn.key);
    if (!defaultColumn) {
      return;
    }

    mergedColumns.push({
      ...defaultColumn,
      visible:
        defaultColumn.locked || defaultColumn.hideable === false
          ? true
          : savedColumn.visible,
    });
    usedKeys.add(defaultColumn.key);
  });

  defaultColumns.forEach((column) => {
    if (!usedKeys.has(column.key)) {
      mergedColumns.push(column);
    }
  });

  return mergedColumns;
}

export function useTableColumnSetting(options: TableColumnSettingOptions) {
  const userStore = useUserStore();
  const minVisibleCount = options.minVisibleCount ?? 1;
  const defaultColumns = computed(() => normalizeColumns(options.columns));
  const userId = computed(() => userStore.userInfo?.userId || 'anonymous');
  const cacheKey = computed(
    () => `${userId.value}:${options.storageKey || 'default'}:${options.pageKey}`,
  );
  const columns = ref<TableColumnSettingColumn[]>([]);

  const visibleColumns = computed(() =>
    columns.value.filter((column) => column.visible !== false),
  );

  const settingColumns = computed(() =>
    columns.value.filter((column) => column.setting !== false),
  );

  const loadColumns = () => {
    const payload = getCacheItem<TableColumnSettingStoragePayload>(cacheKey.value);
    columns.value = mergeColumns(defaultColumns.value, payload);
  };

  const saveColumns = () => {
    const payload: TableColumnSettingStoragePayload = {
      columns: columns.value.map((column, index) => ({
        key: column.key,
        order: index,
        visible: column.visible !== false,
      })),
      updatedAt: Date.now(),
      version: 1,
    };

    setCacheItem(cacheKey.value, payload);
  };

  const setColumnVisible = (key: string, visible: boolean) => {
    const column = columns.value.find((item) => item.key === key);
    if (!column || column.locked || column.hideable === false) {
      return false;
    }

    if (!visible && visibleColumns.value.length <= minVisibleCount) {
      return false;
    }

    column.visible = visible;
    saveColumns();
    return true;
  };

  const moveColumn = (oldIndex: number, newIndex: number) => {
    if (oldIndex === newIndex || oldIndex < 0 || newIndex < 0) {
      return;
    }

    const nextColumns = [...columns.value];
    const movingColumn = nextColumns[oldIndex];
    if (!movingColumn || movingColumn.locked || movingColumn.draggable === false) {
      return;
    }

    nextColumns.splice(oldIndex, 1);
    nextColumns.splice(newIndex, 0, movingColumn);
    columns.value = nextColumns;
    saveColumns();
  };

  const resetOrder = () => {
    const visibleMap = new Map(
      columns.value.map((column) => [column.key, column.visible !== false]),
    );

    columns.value = defaultColumns.value.map((column) => ({
      ...column,
      visible:
        column.locked || column.hideable === false
          ? true
          : (visibleMap.get(column.key) ?? column.visible),
    }));
    saveColumns();
  };

  const resetToDefault = () => {
    columns.value = normalizeColumns(options.columns);
    saveColumns();
  };

  watch(
    () => userId.value,
    () => loadColumns(),
  );

  watch(
    columns,
    () => saveColumns(),
    { deep: true },
  );

  loadColumns();

  return {
    columns,
    loadColumns,
    moveColumn,
    resetOrder,
    resetToDefault,
    saveColumns,
    setColumnVisible,
    settingColumns,
    visibleColumns,
  };
}
