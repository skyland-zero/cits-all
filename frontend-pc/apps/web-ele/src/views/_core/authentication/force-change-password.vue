<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus';

import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import { preferences } from '@vben/preferences';

import { ElMessage } from 'element-plus';

import { changePasswordApi } from '#/api';
import { useAuthStore } from '#/store';

defineOptions({ name: 'ForceChangePassword' });

const router = useRouter();
const authStore = useAuthStore();
const formRef = ref<FormInstance>();
const loading = ref(false);

const formModel = reactive({
  confirmPassword: '',
  newPassword: '',
  oldPassword: '',
});

const rules: FormRules<typeof formModel> = {
  confirmPassword: [
    { message: '请再次输入新密码', required: true, trigger: 'blur' },
    {
      message: '两次输入的密码不一致',
      trigger: 'blur',
      validator: (_rule, value, callback) => {
        if (value !== formModel.newPassword) {
          callback(new Error('两次输入的密码不一致'));
          return;
        }
        callback();
      },
    },
  ],
  newPassword: [
    { message: '请输入新密码', required: true, trigger: 'blur' },
    { message: '新密码至少 8 位', min: 8, trigger: 'blur' },
  ],
  oldPassword: [{ message: '请输入旧密码', required: true, trigger: 'blur' }],
};

async function handleSubmit() {
  const valid = await formRef.value?.validate();
  if (!valid) return;
  loading.value = true;
  try {
    await changePasswordApi({
      newPassword: formModel.newPassword,
      oldPassword: formModel.oldPassword,
    });
    await authStore.fetchUserInfo();
    ElMessage.success('密码修改成功');
    await router.replace(preferences.app.defaultHomePath);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="flex min-h-[420px] items-center justify-center px-4">
    <el-card class="w-full max-w-[420px]" shadow="never">
      <template #header>
        <div>
          <div class="text-lg font-semibold">修改初始密码</div>
          <div class="mt-1 text-sm text-gray-500">
            当前账号需要先修改密码后才能继续使用系统
          </div>
        </div>
      </template>

      <el-form
        ref="formRef"
        label-position="top"
        :model="formModel"
        :rules="rules"
        @submit.prevent="handleSubmit"
      >
        <el-form-item label="旧密码" prop="oldPassword">
          <el-input
            v-model="formModel.oldPassword"
            autocomplete="current-password"
            placeholder="请输入旧密码"
            show-password
            type="password"
          />
        </el-form-item>
        <el-form-item label="新密码" prop="newPassword">
          <el-input
            v-model="formModel.newPassword"
            autocomplete="new-password"
            placeholder="请输入新密码"
            show-password
            type="password"
          />
        </el-form-item>
        <el-form-item label="确认新密码" prop="confirmPassword">
          <el-input
            v-model="formModel.confirmPassword"
            autocomplete="new-password"
            placeholder="请再次输入新密码"
            show-password
            type="password"
          />
        </el-form-item>
        <el-button
          class="mt-2 w-full"
          :loading="loading"
          native-type="submit"
          type="primary"
        >
          确认修改
        </el-button>
      </el-form>
    </el-card>
  </div>
</template>
