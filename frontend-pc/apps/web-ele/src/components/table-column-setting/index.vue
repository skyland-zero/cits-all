<script lang="ts" setup>
import type { Sortable } from '@vben/hooks';
import type { TableColumnSettingColumn } from '#/composables/useTableColumnSetting';

import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue';

import { useSortable } from '@vben/hooks';

import { ElButton, ElCheckbox, ElMessage, ElPopover, ElScrollbar } from 'element-plus';

const props = withDefaults(
  defineProps<{
    buttonText?: string;
    columns: TableColumnSettingColumn[];
    minVisibleCount?: number;
    title?: string;
  }>(),
  {
    buttonText: '列表设置',
    minVisibleCount: 1,
    title: '列表设置',
  },
);

const emit = defineEmits<{
  'reset-order': [];
  'update:columns': [columns: TableColumnSettingColumn[]];
}>();

const visible = ref(false);
const listRef = ref<HTMLElement>();
let sortable: null | Sortable = null;

const settingColumns = computed(() =>
  props.columns.filter((column) => column.setting !== false),
);

const visibleCount = computed(
  () => props.columns.filter((column) => column.visible !== false).length,
);

const cloneColumns = () => props.columns.map((column) => ({ ...column }));

const destroySortable = () => {
  sortable?.destroy();
  sortable = null;
};

const initSortable = async () => {
  if (sortable || !listRef.value || settingColumns.value.length <= 1) {
    return;
  }

  const { initializeSortable } = useSortable(listRef.value, {
    filter: '.table-column-setting__item--drag-disabled',
    handle: '.table-column-setting__drag-handle',
    onEnd: ({
      newIndex,
      oldIndex,
    }: {
      newIndex?: number;
      oldIndex?: number;
    }) => {
      if (oldIndex === undefined || newIndex === undefined || oldIndex === newIndex) {
        return;
      }

      const nextSettingColumns = settingColumns.value.map((column) => ({
        ...column,
      }));
      const movingColumn = nextSettingColumns[oldIndex];
      if (
        !movingColumn ||
        movingColumn.locked ||
        movingColumn.draggable === false
      ) {
        emit('update:columns', cloneColumns());
        return;
      }

      nextSettingColumns.splice(oldIndex, 1);
      nextSettingColumns.splice(newIndex, 0, movingColumn);
      const hiddenSettingColumnKeys = new Set(
        nextSettingColumns.map((column) => column.key),
      );
      const nonSettingColumns = props.columns.filter(
        (column) => !hiddenSettingColumnKeys.has(column.key),
      );

      emit('update:columns', [...nextSettingColumns, ...nonSettingColumns]);
    },
  });

  sortable = await initializeSortable();
};

const close = () => {
  visible.value = false;
};

const isCheckboxDisabled = (column: TableColumnSettingColumn) => {
  if (column.locked || column.hideable === false) {
    return true;
  }

  return column.visible !== false && visibleCount.value <= props.minVisibleCount;
};

const onVisibleChange = (
  column: TableColumnSettingColumn,
  value: boolean | number | string,
) => {
  const visibleValue = Boolean(value);

  if (!visibleValue && visibleCount.value <= props.minVisibleCount) {
    ElMessage.warning(`至少保留 ${props.minVisibleCount} 个显示字段`);
    return;
  }

  const nextColumns = cloneColumns();
  const targetColumn = nextColumns.find((item) => item.key === column.key);
  if (!targetColumn || targetColumn.locked || targetColumn.hideable === false) {
    return;
  }

  targetColumn.visible = visibleValue;
  emit('update:columns', nextColumns);
};

watch(visible, async (value) => {
  if (!value) {
    return;
  }

  await nextTick();
  window.requestAnimationFrame(() => {
    initSortable();
  });
});

onBeforeUnmount(() => {
  destroySortable();
});
</script>

<template>
  <ElPopover
    v-model:visible="visible"
    :hide-after="0"
    :show-after="0"
    placement="bottom-end"
    transition=""
    trigger="click"
    width="360px"
  >
    <template #reference>
      <ElButton plain>{{ buttonText }}</ElButton>
    </template>
    <div class="table-column-setting">
      <div class="table-column-setting__title">{{ title }}</div>
      <div class="table-column-setting__tip">
        拖拽调整字段顺序，取消勾选可隐藏字段
      </div>
      <ElScrollbar max-height="360px">
        <div ref="listRef" class="table-column-setting__list">
          <div
            v-for="column in settingColumns"
            :key="column.key"
            :class="{
              'table-column-setting__item--drag-disabled':
                column.locked || column.draggable === false,
            }"
            class="table-column-setting__item"
          >
            <span class="table-column-setting__drag-handle">⋮⋮</span>
            <ElCheckbox
              :disabled="isCheckboxDisabled(column)"
              :model-value="column.visible !== false"
              @change="(value) => onVisibleChange(column, value)"
            >
              {{ column.label }}
            </ElCheckbox>
          </div>
        </div>
      </ElScrollbar>
      <div class="table-column-setting__footer">
        <ElButton @click="emit('reset-order')">重置顺序</ElButton>
        <ElButton type="primary" @click="close">关闭</ElButton>
      </div>
    </div>
  </ElPopover>
</template>

<style scoped>
.table-column-setting {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.table-column-setting__title {
  color: var(--el-text-color-primary);
  font-size: 15px;
  font-weight: 600;
}

.table-column-setting__tip {
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

.table-column-setting__list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-right: 8px;
}

.table-column-setting__item {
  align-items: center;
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  display: flex;
  gap: 10px;
  min-height: 40px;
  padding: 0 12px;
}

.table-column-setting__drag-handle {
  color: var(--el-text-color-placeholder);
  cursor: move;
  font-size: 16px;
  line-height: 1;
  user-select: none;
}

.table-column-setting__item--drag-disabled .table-column-setting__drag-handle {
  cursor: not-allowed;
  opacity: 0.45;
}

.table-column-setting__footer {
  display: flex;
  justify-content: flex-end;
}
</style>
