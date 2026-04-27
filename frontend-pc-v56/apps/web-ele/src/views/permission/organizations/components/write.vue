<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElForm,
  ElFormItem,
  ElInput,
  ElInputNumber,
  ElTreeSelect,
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
const formLabelWidth = '100px';

const initData = () => ({
  parentId: '',
  name: '',
  code: '',
  description: '',
  sort: 1,
});
const formData = reactive(initData());

const rules = reactive<FormRules>({
  name: [
    { required: true, message: '请输入部门名称', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
});
const treeSelectProps = {
  value: 'id',
  label: 'name',
  children: 'children',
};

const setValues = (currentRow: any) => {
  Object.assign(formData, currentRow);
};

const submit = async () => {
  const formEl = formRef.value;
  if (!formEl) {
    return;
  }
  const valid = await formEl.validate().catch((error) => {
    console.log('valid', error);
  });
  if (valid) {
    return formData;
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
  <ElForm ref="formRef" :model="formData" :rules="rules">
    <ElFormItem
      :label-width="formLabelWidth"
      label="上级部门："
      prop="parentId"
    >
      <ElTreeSelect
        v-model="formData.parentId"
        :data="treeSelectData"
        :props="treeSelectProps"
        :render-after-expand="false"
        check-strictly
        default-expand-all
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="部门名称：" prop="name">
      <ElInput v-model="formData.name" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="部门编码：" prop="code">
      <ElInput v-model="formData.code" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="排序：" prop="order">
      <ElInputNumber v-model="formData.sort" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="说明：" prop="description">
      <ElInput
        v-model="formData.description"
        :rows="2"
        autocomplete="off"
        type="textarea"
      />
    </ElFormItem>
  </ElForm>
</template>
