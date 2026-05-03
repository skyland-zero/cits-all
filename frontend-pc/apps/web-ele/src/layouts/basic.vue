<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts';

import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { AuthenticationLoginExpiredModal } from '@vben/common-ui';
import { VBEN_DOC_URL, VBEN_GITHUB_URL } from '@vben/constants';
import { useWatermark } from '@vben/hooks';
import { BookOpenText, CircleHelp, SvgGithubIcon } from '@vben/icons';
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { openWindow } from '@vben/utils';

import { $t } from '#/locales';
import { useAuthStore } from '#/store';
import { LoginAnnouncementPopup } from '#/components';
import {
  type AnnouncementDto,
  getUnreadAnnouncements,
  markAnnouncementRead,
} from '#/api/system/announcements';
import LoginForm from '#/views/_core/authentication/login.vue';

type AnnouncementNotificationItem = NotificationItem & {
  announcementId?: string;
};

const notifications = ref<AnnouncementNotificationItem[]>([]);

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const accessStore = useAccessStore();
const { destroyWatermark, updateWatermark } = useWatermark();
const showDot = computed(() =>
  notifications.value.some((item) => !item.isRead),
);

const menus = computed(() => [
  {
    handler: () => {
      router.push({ name: 'Profile' });
    },
    icon: 'lucide:user',
    text: $t('page.auth.profile'),
  },
  {
    handler: () => {
      openWindow(VBEN_DOC_URL, {
        target: '_blank',
      });
    },
    icon: BookOpenText,
    text: $t('ui.widgets.document'),
  },
  {
    handler: () => {
      openWindow(VBEN_GITHUB_URL, {
        target: '_blank',
      });
    },
    icon: SvgGithubIcon,
    text: 'GitHub',
  },
  {
    handler: () => {
      openWindow(`${VBEN_GITHUB_URL}/issues`, {
        target: '_blank',
      });
    },
    icon: CircleHelp,
    text: $t('ui.widgets.qa'),
  },
]);

const avatar = computed(() => {
  return userStore.userInfo?.avatar ?? preferences.app.defaultAvatar;
});

async function handleLogout() {
  await authStore.logout(false);
}

function handleNoticeClear() {
  void Promise.all(
    notifications.value
      .filter((item) => !item.isRead && item.announcementId)
      .map((item) => markAnnouncementRead(item.announcementId!)),
  );
  notifications.value = [];
}

async function markRead(id: number | string) {
  const item = notifications.value.find((item) => item.id === id);
  if (item) {
    if (!item.isRead && item.announcementId) {
      await markAnnouncementRead(item.announcementId);
    }
    item.isRead = true;
  }
}

function remove(id: number | string) {
  void markRead(id);
  notifications.value = notifications.value.filter((item) => item.id !== id);
}

function handleMakeAll() {
  void Promise.all(
    notifications.value
      .filter((item) => !item.isRead && item.announcementId)
      .map((item) => markAnnouncementRead(item.announcementId!)),
  );
  notifications.value.forEach((item) => (item.isRead = true));
}

function mapAnnouncementNotification(
  item: AnnouncementDto,
): AnnouncementNotificationItem {
  return {
    id: `announcement:${item.id}`,
    announcementId: item.id,
    avatar: 'https://avatar.vercel.sh/announcement.svg?text=公告',
    date: item.publishTime || item.creationTime || '',
    isRead: false,
    message: item.summary || '系统公告',
    title: item.title,
  };
}

async function fetchNotifications() {
  const announcements = await getUnreadAnnouncements();
  notifications.value = announcements.map(mapAnnouncementNotification);
}

onMounted(() => {
  void fetchNotifications();
});
watch(
  () => ({
    enable: preferences.app.watermark,
    content: preferences.app.watermarkContent,
  }),
  async ({ enable, content }) => {
    if (enable) {
      await updateWatermark({
        content:
          content ||
          `${userStore.userInfo?.username} - ${userStore.userInfo?.realName}`,
      });
    } else {
      destroyWatermark();
    }
  },
  {
    immediate: true,
  },
);
</script>

<template>
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userStore.userInfo?.realName"
        description="ann.vben@gmail.com"
        tag-text="Pro"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        :dot="showDot"
        :notifications="notifications"
        @clear="handleNoticeClear"
        @read="(item) => item.id && markRead(item.id)"
        @remove="(item) => item.id && remove(item.id)"
        @make-all="handleMakeAll"
      />
    </template>
    <template #extra>
      <AuthenticationLoginExpiredModal
        v-model:open="accessStore.loginExpired"
        :avatar
      >
        <LoginForm />
      </AuthenticationLoginExpiredModal>
      <LoginAnnouncementPopup @changed="fetchNotifications" />
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
</template>
