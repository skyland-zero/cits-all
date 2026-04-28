<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElForm,
  ElFormItem,
  ElInput,
  ElRadio,
  ElRadioGroup,
  ElScrollbar,
  ElTree,
  type FormInstance,
  type FormRules,
  type TreeInstance,
} from 'element-plus';

import { getLitePageApi } from '#/api/permission/menus';
import { getTreeSelectApi } from '#/api/permission/permissions';

const props = defineProps({
  treeSelectData: {
    type: Array as PropType<any[]>,
    default: () => [],
  },
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});
const formRef = ref<FormInstance>();
const formLabelWidth = '140px';
const loading = ref(false);
const radio = ref(1);
const treeRef = ref<TreeInstance>();
const treeData = ref<any>([]);
const checkKeys = ref<any>([]);
let menuId: string = '';
const defaultProps = {
  children: 'children',
  label: 'label',
};

const initData = () => ({
  parentId: null as null | string,
  name: '',
  path: '',
  icon: '',
  order: 1,
  enabled: true,
  hideInMenu: false,
  type: 3,
});
const formData = reactive(initData());

const rules = reactive<FormRules>({
  name: [
    {
      required: true,
      message: '请输入分组名称',
      trigger: 'blur',
    },
    {
      min: 1,
      max: 50,
      message: '长度在 1 到 50 个字符',
      trigger: 'blur',
    },
  ],
});

const processTree = (nodes: any[], checkedIds: string[]) => {
  const idSet = new Set(checkedIds);
  const processNodes = (nodes: any[]) => {
    return nodes.map((node) => {
      // 深拷贝节点，避免修改原始数据
      const newNode = { ...node };
      const isLeaf = !newNode.children || newNode.children.length === 0;
      if (isLeaf) {
        // 末级节点：检查ID并设置disabled
        newNode.disabled = idSet.has(newNode.value);
      } else {
        // 递归处理子节点
        newNode.children = processNodes(newNode.children);
      }
      return newNode;
    });
  };
  return processNodes(nodes);
};

const fetchAll = async () => {
  const res = await getTreeSelectApi();
  treeData.value = res.items;
};

const fetchSelected = async (id: string) => {
  console.log(id);
  const res = await getLitePageApi({ maxResultCount: 1000 }, { parentId: id });
  checkKeys.value = res.items.map((x: any) => x.path);
  console.log('checkKeys.value', checkKeys.value);
  treeData.value = processTree(treeData.value, checkKeys.value);
  console.log('treeData.value', treeData.value);
  // treeRef.value!.setCheckedKeys(checkKeys.value, false);
};

const fetchData = async (currentRow: any) => {
  loading.value = true;
  await fetchAll();
  await fetchSelected(currentRow.id);
  loading.value = false;
};

const submit = async () => {
  if (radio.value === 1) {
    const data = treeRef.value?.getCheckedNodes(true);
    return data?.map((x) => {
      return {
        name: x.label,
        path: x.value,
        parentId: menuId,
        enabled: true,
        type: 3,
      };
    });
  } else {
    const formEl = formRef.value;
    if (!formEl) {
      return;
    }
    const valid = await formEl.validate().catch((error) => {
      console.log('valid', error);
    });
    if (valid) {
      return [formData];
    }
  }
};

/**
 * 放在defineExpose之前，其他内容之后
 */
watch(
  () => props.currentRow,
  (currentRow) => {
    console.log('currentRow', currentRow);
    if (!currentRow) return;
    menuId = currentRow.id;
    formData.parentId = menuId;
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
  <ElForm ref="formRef" :model="formData" :rules="rules" v-loading="loading">
    <ElFormItem :label-width="formLabelWidth" label="新增类型：">
      <ElRadioGroup v-model="radio">
        <ElRadio :value="1">从权限列表选择</ElRadio>
        <ElRadio :value="2">直接新增</ElRadio>
      </ElRadioGroup>
    </ElFormItem>
    <div v-if="radio === 1">
      <ElScrollbar height="400px">
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
        >
          <template #default="{ node, data }">
            <div class="custom-tree-node">
              <span>{{ node.label }}</span>
              <div>
                <ElText class="mx-1" size="small" type="primary">
                  {{ data.value }}
                </ElText>
                <!-- <el-button link type="primary"> Append </el-button> -->
              </div>
            </div>
          </template>
        </ElTree>
      </ElScrollbar>
    </div>
    <div v-if="radio === 2">
      <ElFormItem :label-width="formLabelWidth" label="权限名称：" prop="name">
        <ElInput v-model="formData.name" autocomplete="off" />
      </ElFormItem>
      <ElFormItem :label-width="formLabelWidth" label="权限编码：" prop="path">
        <ElInput v-model="formData.path" autocomplete="off" />
      </ElFormItem>
    </div>
  </ElForm>
</template>

<style scoped>
.custom-tree-node {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: space-between;
  padding-right: 8px;
  font-size: 14px;
}
</style>
