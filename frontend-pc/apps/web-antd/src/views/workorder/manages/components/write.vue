<script lang="ts" setup>
import { reactive, ref, watch } from 'vue';

import {
  Form,
  FormItem,
  Input,
  Select,
  SelectOption,
  DatePicker,
  Upload,
  Button,
  message,
} from 'ant-design-vue';

import { requestClient } from '#/api/request';
import { WorkOrderPriority } from '../enums';

const props = defineProps({
  currentRow: {
    type: Object,
    default: () => null,
  },
});

const formRef = ref();
const fileList = ref<any[]>([]);

const formData = reactive({
  id: '',
  title: '',
  priority: WorkOrderPriority.Medium,
  description: '',
  location: '',
  contactName: '',
  contactPhone: '',
  deadlineTime: undefined,
  attachments: '[]',
});

const priorityOptions = [
  { label: '低', value: WorkOrderPriority.Low },
  { label: '中', value: WorkOrderPriority.Medium },
  { label: '高', value: WorkOrderPriority.High },
  { label: '紧急', value: WorkOrderPriority.Critical },
];

const handleUpload = async (options: any) => {
  const { file, onSuccess, onError } = options;
  try {
    const response = await requestClient.upload('/basic/upload/Upload/single', {
      file,
    });
    onSuccess(response);
  } catch (error) {
    onError(error);
  }
};

const setValues = (currentRow: any) => {
  Object.assign(formData, currentRow);
  if (currentRow.attachmentList && Array.isArray(currentRow.attachmentList)) {
    fileList.value = currentRow.attachmentList.map((item: any) => ({
      uid: item.id,
      name: item.originalName,
      url: item.url || item.relativePath,
      status: 'done',
    }));
  }
};

watch(
  () => props.currentRow,
  (val) => {
    if (val) {
      setValues(val);
    }
  },
  { immediate: true },
);

const submit = async () => {
  try {
    await formRef.value.validate();
    const attachmentsData = fileList.value
      .filter((f) => f.status === 'done')
      .map((f: any) => ({
        id: f.uid || f.response?.id,
        name: f.name,
        url: f.url || f.response?.url,
      }));
    formData.attachments = JSON.stringify(attachmentsData);
    return formData;
  } catch (error) {
    return null;
  }
};

defineExpose({ submit });
</script>

<template>
  <Form ref="formRef" :model="formData" layout="vertical">
    <FormItem label="标题" name="title" :rules="[{ required: true, message: '请输入标题' }]">
      <Input v-model:value="formData.title" placeholder="请输入工单标题" />
    </FormItem>
    <FormItem label="优先级" name="priority">
      <Select v-model:value="formData.priority">
        <SelectOption v-for="opt in priorityOptions" :key="opt.value" :value="opt.value">
          {{ opt.label }}
        </SelectOption>
      </Select>
    </FormItem>
    <FormItem label="地点" name="location">
      <Input v-model:value="formData.location" placeholder="故障/作业地点" />
    </FormItem>
    <div class="grid grid-cols-2 gap-4">
      <FormItem label="联系人" name="contactName">
        <Input v-model:value="formData.contactName" />
      </FormItem>
      <FormItem label="联系电话" name="contactPhone">
        <Input v-model:value="formData.contactPhone" />
      </FormItem>
    </div>
    <FormItem label="截止时间" name="deadlineTime">
      <DatePicker v-model:value="formData.deadlineTime" show-time value-format="YYYY-MM-DD HH:mm:ss" class="w-full" />
    </FormItem>
    <FormItem label="描述" name="description">
      <Input.TextArea v-model:value="formData.description" :rows="4" />
    </FormItem>
    <FormItem label="附件">
      <Upload v-model:file-list="fileList" :custom-request="handleUpload" multiple>
        <Button>点击上传</Button>
      </Upload>
    </FormItem>
  </Form>
</template>
