<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElCol,
  ElForm,
  ElFormItem,
  ElInput,
  ElRow,
  ElSwitch,
  type FormInstance,
  type FormRules,
} from 'element-plus';

const props = defineProps({
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});
const formRef = ref<FormInstance>();
const formLabelWidth = '160px';

const initData = () => ({
  name: '',
  code: '',
  isDefault: false,
  isStatic: false,
  description: '',
  menuIds: [],
});
const formData = reactive(initData());

const rules = reactive<FormRules>({
  name: [
    { required: true, message: '请输入角色名称', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
  code: [
    { required: true, message: '请输入角色唯一编码', trigger: 'blur' },
    { min: 1, max: 50, message: '长度在 1 到 50 个字符', trigger: 'blur' },
  ],
});

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
    <ElFormItem :label-width="formLabelWidth" label="角色名称：" prop="name">
      <ElInput v-model="formData.name" autocomplete="off" />
    </ElFormItem>
    <ElFormItem
      :label-width="formLabelWidth"
      label="角色唯一编码："
      prop="code"
    >
      <ElInput v-model="formData.code" autocomplete="off" />
    </ElFormItem>
    <ElFormItem :label-width="formLabelWidth" label="说明：" prop="description">
      <ElInput
        v-model="formData.description"
        :rows="2"
        autocomplete="off"
        textarea
      />
    </ElFormItem>
    <ElRow>
      <ElCol :span="12">
        <ElFormItem
          :label-width="formLabelWidth"
          label="是否默认角色："
          prop="isDefault"
        >
          <ElSwitch
            v-model="formData.isDefault"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
      <ElCol :span="12">
        <ElFormItem
          :label-width="formLabelWidth"
          label="是否固定角色："
          prop="isStatic"
        >
          <ElSwitch
            v-model="formData.isStatic"
            active-text="是"
            inactive-text="否"
            inline-prompt
          />
        </ElFormItem>
      </ElCol>
    </ElRow>
  </ElForm>
</template>
