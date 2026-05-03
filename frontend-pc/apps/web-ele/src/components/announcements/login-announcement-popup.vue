<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';

import { ElButton, ElDialog, ElTag } from 'element-plus';

import {
  type AnnouncementDto,
  getLoginPopupAnnouncements,
  markAnnouncementRead,
} from '#/api/system/announcements';

import RichHtml from '../rich-text/rich-html.vue';

const announcements = ref<AnnouncementDto[]>([]);
const currentIndex = ref(0);
const visible = ref(false);
const loading = ref(false);

const current = computed(() => announcements.value[currentIndex.value]);
const hasNext = computed(
  () => currentIndex.value < announcements.value.length - 1,
);

const emit = defineEmits<{
  changed: [];
}>();

function getPriorityLabel(priority: number) {
  if (priority >= 20) return '紧急';
  if (priority >= 10) return '重要';
  return '普通';
}

function getPriorityType(priority: number) {
  if (priority >= 20) return 'danger';
  if (priority >= 10) return 'warning';
  return 'primary';
}

async function fetchAnnouncements() {
  announcements.value = await getLoginPopupAnnouncements();
  currentIndex.value = 0;
  visible.value = announcements.value.length > 0;
}

async function markCurrentRead() {
  const item = current.value;
  if (!item) return;
  await markAnnouncementRead(item.id);
}

async function handleNext() {
  loading.value = true;
  try {
    await markCurrentRead();
    if (hasNext.value) {
      currentIndex.value += 1;
    } else {
      visible.value = false;
      announcements.value = [];
    }
    emit('changed');
  } finally {
    loading.value = false;
  }
}

async function handleMarkAllRead() {
  loading.value = true;
  try {
    await Promise.all(
      announcements.value.map((item) => markAnnouncementRead(item.id)),
    );
    visible.value = false;
    announcements.value = [];
    emit('changed');
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  void fetchAnnouncements();
});
</script>

<template>
  <ElDialog
    v-model="visible"
    :close-on-click-modal="false"
    title="系统公告"
    width="760px"
  >
    <div v-if="current" class="space-y-4">
      <div>
        <div class="mb-2 flex items-center gap-2">
          <ElTag :type="getPriorityType(current.priority)">
            {{ getPriorityLabel(current.priority) }}
          </ElTag>
          <span class="text-xs text-gray-500">
            {{ current.publishTime || current.creationTime }}
          </span>
          <span class="ml-auto text-xs text-gray-400">
            {{ currentIndex + 1 }} / {{ announcements.length }}
          </span>
        </div>
        <h2 class="text-xl font-semibold">{{ current.title }}</h2>
        <p
          v-if="current.summary"
          class="mt-2 rounded bg-gray-50 p-3 text-sm text-gray-600 dark:bg-gray-800"
        >
          {{ current.summary }}
        </p>
      </div>
      <div class="max-h-[52vh] overflow-auto pr-2">
        <RichHtml :html="current.contentHtml" />
      </div>
    </div>

    <template #footer>
      <ElButton :loading="loading" @click="handleMarkAllRead"
        >全部标为已读</ElButton
      >
      <ElButton :loading="loading" type="primary" @click="handleNext">
        {{ hasNext ? '下一条' : '我知道了' }}
      </ElButton>
    </template>
  </ElDialog>
</template>
