<!-- eslint-disable no-console -->
<script lang="ts" setup>
import type { Nullable } from '@vben/types';

import { type PropType, reactive, ref, watch } from 'vue';

import {
  ElDatePicker,
  ElForm,
  ElFormItem,
  ElInput,
  ElOption,
  ElSelect,
  type FormInstance,
  type FormRules,
} from 'element-plus';

import { WorkOrderPriority } from '../enums';

const props = defineProps({
  currentRow: {
    type: Object as PropType<Nullable<any>>,
    default: () => null,
  },
});
const formRef = ref<FormInstance>();
const formLabelWidth = '120px';
const loading = ref(false);

const initData = () => ({
  id: '',
  title: '',
  priority: WorkOrderPriority.Medium, // Default to Medium
  description: '',
  location: '',
  contactName: '',
  contactPhone: '',
  deadlineTime: '',
});
const formData = reactive(initData());

const priorityOptions = [
  { label: '低', value: WorkOrderPriority.Low },
  { label: '中', value: WorkOrderPriority.Medium },
  { label: '高', value: WorkOrderPriority.High },
  { label: '紧急', value: WorkOrderPriority.Critical },
];

const rules = reactive<FormRules>({
  title: [
    { required: true, message: '请输入工单标题', trigger: 'blur' },
    { min: 1, max: 200, message: '长度在 1 到 200 个字符', trigger: 'blur' },
  ],
  priority: [{ required: true, message: '请选择优先级', trigger: 'change' }],
  contactName: [{ max: 50, message: '长度不能超过 50 个字符', trigger: 'blur' }],
  contactPhone: [{ max: 20, message: '长度不能超过 20 个字符', trigger: 'blur' }],
  location: [{ max: 200, message: '长度不能超过 200 个字符', trigger: 'blur' }],
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
    if (!currentRow) {
      Object.assign(formData, initData());
      return;
    }
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
    <ElFormItem :label-width="formLabelWidth" label="标题：" prop="title">
      <ElInput v-model="formData.title" autocomplete="off" placeholder="请输入工单标题" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="优先级：" prop="priority">
      <ElSelect v-model="formData.priority" placeholder="请选择优先级" style="width: 100%">
        <ElOption v-for="item in priorityOptions" :key="item.value" :label="item.label" :value="item.value" />
      </ElSelect>
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="地点：" prop="location">
      <ElInput v-model="formData.location" autocomplete="off" placeholder="故障/作业地点" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="联系人：" prop="contactName">
      <ElInput v-model="formData.contactName" autocomplete="off" placeholder="联系人姓名" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="联系电话：" prop="contactPhone">
      <ElInput v-model="formData.contactPhone" autocomplete="off" placeholder="联系人电话" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="截止时间：" prop="deadlineTime">
      <ElDatePicker v-model="formData.deadlineTime" type="datetime" placeholder="选择处理截止时间" style="width: 100%"
        value-format="YYYY-MM-DD HH:mm:ss" />
    </ElFormItem>

    <ElFormItem :label-width="formLabelWidth" label="描述：" prop="description">
      <ElInput v-model="formData.description" type="textarea" :rows="4" placeholder="请输入工单描述" />
    </ElFormItem>
  </ElForm>
</template>
