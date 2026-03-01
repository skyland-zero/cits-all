<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElCol,
  ElForm,
  ElFormItem,
  ElInput,
  ElInputNumber,
  ElRow,
  ElSwitch,
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
  parentId: null,
  name: '',
  path: '',
  description: '',
  sort: 1,
  enabled: true,
});
const formData = reactive(initData());

const rules = reactive<FormRules>({
  name: [
    { required: true, message: '请输入页面名称', trigger: 'blur' },
    { min: 1, max: 20, message: '长度在 1 到 20 个字符', trigger: 'blur' },
  ],
  path: [
    { required: true, message: '请输入页面路径', trigger: 'blur' },
    { min: 1, max: 200, message: '长度在 1 到 200 个字符', trigger: 'blur' },
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
      label="上级页面："
      prop="parentId"
    >
      <ElTreeSelect
        v-model="formData.parentId"
        :data="treeSelectData"
        :props="treeSelectProps"
        :render-after-expand="false"
        check-strictly
      />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="页面名称：" prop="name">
      <ElInput v-model="formData.name" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="页面路径：" prop="path">
      <ElInput
        v-model="formData.path"
        autocomplete="off"
        placeholder="页面的路径，需要去掉 views/ 和 .vue"
      />
    </ElFormItem>
    <ElRow>
      <ElCol :span="12">
        <ElFormItem :label-width="formLabelWidth" label="排序：" prop="sort">
          <ElInputNumber v-model="formData.sort" />
        </ElFormItem>
      </ElCol>
      <ElCol :span="12">
        <ElFormItem :label-width="formLabelWidth" label="启用：" prop="enabled">
          <ElSwitch
            v-model="formData.enabled"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
    </ElRow>
    <ElFormItem
      :label-width="formLabelWidth"
      label="页面说明："
      prop="description"
    >
      <ElInput
        v-model="formData.description"
        :rows="2"
        autocomplete="off"
        type="textarea"
      />
    </ElFormItem>
  </ElForm>
</template>
