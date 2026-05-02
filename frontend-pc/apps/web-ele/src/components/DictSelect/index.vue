<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { ElSelect, ElOption } from 'element-plus';
import { getDictItemsByCode } from '#/api/system/dict';

const props = defineProps({
  dictCode: {
    type: String,
    required: true,
  },
  modelValue: {
    type: [String, Number],
    default: '',
  },
  placeholder: {
    type: String,
    default: '请选择',
  },
  clearable: {
    type: Boolean,
    default: true,
  },
});

const emit = defineEmits(['update:modelValue', 'change']);

const options = ref<any[]>([]);
const loading = ref(false);

const fetchOptions = async () => {
  if (!props.dictCode) return;
  try {
    loading.value = true;
    options.value = await getDictItemsByCode(props.dictCode);
  } catch (error) {
    console.error('Failed to fetch dict items:', error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchOptions();
});

watch(
  () => props.dictCode,
  () => {
    fetchOptions();
  }
);

const onChange = (val: any) => {
  emit('update:modelValue', val);
  emit('change', val);
};
</script>

<template>
  <ElSelect
    :model-value="modelValue"
    :placeholder="placeholder"
    :clearable="clearable"
    :loading="loading"
    @update:model-value="$emit('update:modelValue', $event)"
    @change="onChange"
  >
    <ElOption
      v-for="item in options"
      :key="item.value"
      :label="item.label"
      :value="item.value"
    />
  </ElSelect>
</template>
