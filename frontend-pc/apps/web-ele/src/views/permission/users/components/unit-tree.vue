<!-- eslint-disable no-console -->
<script lang="ts" setup>
import { defineEmits, onMounted, ref, watch } from 'vue';

import { MdiSearch } from '@vben/icons';

import {
  ElContainer,
  ElHeader,
  ElInput,
  ElMain,
  ElScrollbar,
  ElTree,
  type TreeInstance,
} from 'element-plus';

import { getTreeApi } from '#/api/permission/organizations';

const emitEvents = defineEmits(['currentChange']);
const filterText = ref('');
const treeRef = ref<TreeInstance>();
const loading = ref(true);
const treeData = ref<any>([]);

const defaultProps = {
  children: 'children',
  label: 'name',
  value: 'id',
};

// 定义可触发的事件

const filterNode = (value: string, data: any) => {
  if (!value) return true;
  return data.name.includes(value);
};

const fetchData = async () => {
  loading.value = true;
  const res = await getTreeApi();
  treeData.value = res.items;
  loading.value = false;
};

watch(filterText, (val) => {
  // eslint-disable-next-line unicorn/no-array-callback-reference
  treeRef.value!.filter(val);
});

const currentChange = (data: any) => {
  emitEvents('currentChange', data); // 触发事件
};

const onReset = async () => {
  treeRef.value!.setCurrentKey(undefined, false);
};

onMounted(() => {
  fetchData();
});

defineExpose({
  onReset,
});
</script>

<template>
  <ElContainer class="h-full w-full pb-[18px] pl-[8px] pr-[8px] pt-[18px]">
    <ElHeader>
      <ElInput
        v-model="filterText"
        :prefix-icon="MdiSearch"
        placeholder="请输入部门名称"
        style="width: 100%"
      />
    </ElHeader>
    <ElMain class="h-full w-full">
      <ElScrollbar height="100%">
        <ElTree
          ref="treeRef"
          :data="treeData"
          :expand-on-click-node="false"
          :filter-node-method="filterNode"
          :props="defaultProps"
          class="filter-tree"
          default-expand-all
          highlight-current
          node-key="id"
          style="max-width: 600px"
          @current-change="currentChange"
        />
      </ElScrollbar>
    </ElMain>
  </ElContainer>
</template>
