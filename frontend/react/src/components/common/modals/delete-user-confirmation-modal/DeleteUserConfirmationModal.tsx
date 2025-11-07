import Modal from "../modal/Modal";
import type { DeleteUserConfirmationModalProps } from "./types";

const DeleteUserConfirmationModal = ({
  isOpen = false,
  onClose,
  onConfirm,
  participant,
}: DeleteUserConfirmationModalProps) => (
  <Modal
    title="Remove Participant"
    description="You are one step away from permanently removing the user from the room."
    iconName="none"
    isOpen={isOpen}
    onClose={onClose}
    onConfirm={onConfirm}
    cancelButtonText="Cancel"
    confirmButtonText="Confirm"
  >
    <p>{`Are you sure you want to delete participant ${participant.firstName} ${participant.lastName}?`}</p>
  </Modal>
);
export default DeleteUserConfirmationModal;
