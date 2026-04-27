<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElForm,
  ElFormItem,
  ElInput,
  type FormInstance,
  type FormRules,
} from 'element-plus';

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

const initData = () => ({
  parentId: null,
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
  path: [
    {
      required: true,
      message: '请输入权限编码',
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

const submit = async () => {
  const formEl = formRef.value;
  if (!formEl) {
    return;
  }
  const valid = await formEl.validate().catch(() => {});
  if (valid) {
    return formData;
  }
};

const setValues = (currentRow: any) => {
  Object.assign(formData, currentRow);
};

/**
 * 放在defineExpose之前，其他内容之后
 */
watch(
  () => props.currentRow,
  (currentRow) => {
    console.log('currentRow', currentRow);
    if (!currentRow) return;
    setValues(currentRow);
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
    <ElFormItem :label-width="formLabelWidth" label="权限名称：" prop="name">
      <ElInput v-model="formData.name" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="权限编码：" prop="path">
      <ElInput v-model="formData.path" autocomplete="off" />
    </ElFormItem>
  </ElForm>
</template>
