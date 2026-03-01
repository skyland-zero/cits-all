<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';
import type { TreeInstance } from 'element-plus';

import { type PropType, ref, watch } from 'vue';

import { ElTree } from 'element-plus';

import { getTreeSelectApi } from '#/api/permission/menus';
import { getSelectedMenuIdsApi } from '#/api/permission/roles';

const props = defineProps({
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});
const treeRef = ref<TreeInstance>();
const treeData = ref<any>([]);
const checkKeys = ref<any>([]);
const loading = ref(true);

const fetchAll = async () => {
  const res = await getTreeSelectApi();
  treeData.value = res.items;
};

const fetchSelected = async (id: string) => {
  console.log(id);
  const res = await getSelectedMenuIdsApi(id);
  checkKeys.value = res.items;
  treeRef.value!.setCheckedKeys(checkKeys.value, true);
};

const fetchData = async (currentRow: any) => {
  loading.value = true;
  await fetchAll();
  await fetchSelected(currentRow.id);
  loading.value = false;
};

const defaultProps = {
  children: 'children',
  label: 'label',
};

const submit = async () => {
  const treeEl = treeRef.value;
  if (!treeEl) {
    return;
  }
  console.log('getCheckedNodes', treeEl.getCheckedNodes(false, true));
  return treeEl.getCheckedNodes(false, false) as any[];
};

/**
 * 放在defineExpose之前，其他内容之后
 */
watch(
  () => props.currentRow,
  (currentRow) => {
    console.log('currentRow', currentRow);
    if (!currentRow) return;
    fetchData(currentRow);
  },
  {
    deep: true,
    immediate: true,
  },
);
defineExpose({
  submit,
});
</script>

<template>
  <ElTree
    ref="treeRef"
    :data="treeData"
    :props="defaultProps"
    default-expand-all
    highlight-current
    node-key="value"
    show-checkbox
    style="max-width: 100%; min-height: 200px"
    v-loading="loading"
  />

  <!-- <div class="mt-2">
    <ElButton @click="getCheckedKeys">get by key</ElButton>
    <ElButton @click="resetChecked">reset</ElButton>
  </div> -->
</template>
