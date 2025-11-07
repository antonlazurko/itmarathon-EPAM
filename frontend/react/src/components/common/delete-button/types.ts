import type { GiftIdeaField } from "../gift-idea/types";

export const BUTTON_COLORS = {
  GREEN: "green",
  WHITE: "white",
} as const;

export type ButtonColor = (typeof BUTTON_COLORS)[keyof typeof BUTTON_COLORS];

export type IconName = "delete" | "link";

export interface DeleteButtonProps {
  onDeleteButtonClick: () => void;
}

export interface DeleteUserRequest {
  id: number;
  userCode: string;
}

export interface DeleteUserResponse {
  id: number;
  roomId: number;
  createdOn: string;
  modifiedOn: string;
  isAdmin: boolean;
  firstName: string;
  lastName: string;
  userCode: string;
  phone: string;
  email: string;
  deliveryInfo: string;
  giftToUserId: number;
  wantSurprise: boolean;
  interests: string;
  wishList: GiftIdeaField[];
}
